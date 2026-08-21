using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamChatInator.ApiModels;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Services;
using StreamChatInator.Services.Twitch;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace StreamChatInator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Device-code login attempts, keyed by the id handed back to the
        // frontend so it can poll for completion. The device code is single-use
        // (removed once the login resolves) and short-lived, so concurrent
        // logins don't clobber each other and an in-flight login dies cleanly.
        private static readonly ConcurrentDictionary<string, (string DeviceCode, DateTime ExpiresAt)> _deviceAttempts = new();

        private const string Scopes = "chat:edit chat:read";

        private readonly DatabaseContext _db;
        private readonly TwitchAuthService _twitchAuthService;
        private readonly TwitchTokenService _twitchTokenService;
        private readonly AccessControlService _lanAccess;
        private readonly TwitchOAuthClient _twitchOAuthClient;

        public AuthController(DatabaseContext db, TwitchOAuthClient twitchOAuthClient, TwitchAuthService twitchAuthService, TwitchTokenService twitchTokenService, AccessControlService lanAccess)
        {
            _db = db;
            _twitchOAuthClient = twitchOAuthClient;
            _twitchAuthService = twitchAuthService;
            _twitchTokenService = twitchTokenService;
            _lanAccess = lanAccess;
        }

        /// <summary>
        /// Starts a Twitch device code login: asks Twitch for a code, stores the
        /// device code server-side, and returns everything the UI needs to show
        /// the user (verification URL + code) plus the polling id.
        /// </summary>
        [HttpPost("beginDeviceLogin")]
        public async Task<IActionResult> BeginDeviceLogin()
        {
            var response = await _twitchOAuthClient.RequestDeviceCodeAsync(Scopes);
            if (response == null || string.IsNullOrEmpty(response.DeviceCode))
            {
                return ResponseHelper.Response502("twitch_unavailable");
            }

            PruneExpiredAttempts();

            var id = new Guid().ToString();
            _deviceAttempts[id] = (response.DeviceCode, DateTime.UtcNow.AddSeconds(Math.Max(response.ExpiresIn, 60)));

            return Ok(new
            {
                id,
                userCode = response.UserCode,
                verificationUri = response.VerificationUri,
                expiresIn = response.ExpiresIn,
                interval = response.Interval,
            });
        }

        /// <summary>
        /// Called by the UI to see whether the device login completed. Each call
        /// performs a single Twitch poll; the UI is expected to poll roughly every
        /// <c>interval</c> seconds. On success the tokens are persisted and the
        /// attempt is removed.
        /// </summary>
        [HttpGet("deviceStatus")]
        public async Task<IActionResult> DeviceStatus(string? id)
        {
            if (string.IsNullOrEmpty(id) || !_deviceAttempts.TryGetValue(id, out var attempt))
            {
                return ResponseHelper.OkStatus("expired");
            }

            if (attempt.ExpiresAt < DateTime.UtcNow)
            {
                _deviceAttempts.TryRemove(id, out _);
                return ResponseHelper.OkStatus("expired");
            }

            var result = await _twitchOAuthClient.PollDeviceCodeAsync(attempt.DeviceCode, Scopes);
            if (result.Status == DevicePollStatus.Pending)
            {
                return ResponseHelper.OkStatus("pending");
            }
            if (result.Status == DevicePollStatus.Failed)
            {
                _deviceAttempts.TryRemove(id, out _);
                return ResponseHelper.OkStatus("failed");
            }

            _deviceAttempts.TryRemove(id, out _);

            var token = result.Token!;
            var validation = await _twitchOAuthClient.ValidateTokenAsync(token.AccessToken);
            if (validation == null)
            {
                return ResponseHelper.OkStatus("failed");
            }

            // Persist the rotation details first, then publish the new token
            // through TwitchTokenService last, so watchers never see a new
            // token alongside a stale expiry.
            if (!string.IsNullOrEmpty(token.RefreshToken))
            {
                _db.SetSettingsValue(SettingValue.SettingOAuthRefreshToken, token.RefreshToken);
            }
            _db.SetSettingsValue(SettingValue.SettingOAuthTokenExpiresAt, DateTime.UtcNow.AddSeconds(token.ExpiresIn).ToString("o"));
            _db.SetSettingsValue(SettingValue.SettingUserName, validation.Login);
            _db.SaveChanges();

            _twitchTokenService.SetAccessToken(token.AccessToken);
            _twitchAuthService.SignalLogin();

            return ResponseHelper.OkStatusUsername("ok", validation.Login);
        }


        /// <summary>
        /// Whether this browser is allowed in. When Auth:Enabled=false this is
        /// always true, which is what lets the frontend skip the login screen
        /// entirely when gating is opted out.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new
            {
                authenticated = !_lanAccess.Enabled || User.Identity?.IsAuthenticated == true,
                authenticationEnabled = _lanAccess.Enabled,
            });
        }

        /// <summary>
        /// Validates the shared PIN and issues a session cookie. A few bad
        /// attempts briefly lock out further tries to make a short PIN harder
        /// to brute-force over the network.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("pinLogin")]
        public async Task<IActionResult> PinLogin([FromBody] PinLoginRequest request)
        {
            if (!_lanAccess.Enabled) return NotFound();

            // Lockout is per client IP so one device hammering the PIN doesn't
            // lock everyone else out of the UI. A null IP is fine: AccessControlService
            // buckets all such clients together under a dedicated "unknown" key.
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (_lanAccess.IsLockedOut(clientIp)) return StatusCode(429, new { error = "too_many_attempts" });
            if (!_lanAccess.ValidatePin(request.Pin))
            {
                _lanAccess.RegisterFailure(clientIp);
                return Unauthorized(new { error = "invalid_pin" });
            }
            _lanAccess.ResetFailures(clientIp);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return Ok(new { ok = true });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        /// <summary>
        /// Removes abandoned device-code attempts so the dictionary doesn't grow
        /// unbounded when a login is started but never polled to completion.
        /// </summary>
        private static void PruneExpiredAttempts()
        {
            foreach (var kvp in _deviceAttempts)
            {
                if (kvp.Value.ExpiresAt < DateTime.UtcNow)
                {
                    _deviceAttempts.TryRemove(kvp.Key, out _);
                }
            }
        }
    }
}