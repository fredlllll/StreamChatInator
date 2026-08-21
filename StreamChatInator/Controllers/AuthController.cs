using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StreamChatInator.ApiModels;
using StreamChatInator.Auth;
using StreamChatInator.Services;
using StreamChatInator.Services.Twitch;
using StreamChatInator.Services.Twitch.Settings;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace StreamChatInator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Device-code login attempts, keyed by the id handed back to the
        // frontend so it can poll for completion.
        private static readonly ConcurrentDictionary<string, DeviceLoginAttempt> _deviceAttempts = new();

        private const string Scopes = "chat:edit chat:read";

        private readonly TwitchTokenSettingService _twitchTokenService;
        private readonly TwitchRefreshTokenSettingService _twitchRefreshTokenService;
        private readonly TwitchTokenExpiresAtSettingService _twitchTokenExpiresAtService;
        private readonly TwitchUsernameService _twitchUsernameService;
        private readonly AccessControlService _lanAccess;
        private readonly TwitchOAuthService _twitchOAuthClient;
        private readonly ILogger<AuthController> _logger;

        public AuthController(TwitchOAuthService twitchOAuthClient, TwitchTokenSettingService twitchTokenService, TwitchRefreshTokenSettingService twitchRefreshTokenService, TwitchTokenExpiresAtSettingService twitchTokenExpiresAtService, TwitchUsernameService twitchUsernameService, AccessControlService lanAccess, ILogger<AuthController> logger)
        {
            _twitchOAuthClient = twitchOAuthClient;
            _twitchTokenService = twitchTokenService;
            _twitchRefreshTokenService = twitchRefreshTokenService;
            _twitchTokenExpiresAtService = twitchTokenExpiresAtService;
            _twitchUsernameService = twitchUsernameService;
            _lanAccess = lanAccess;
            _logger = logger;
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

            var id = Guid.NewGuid().ToString();
            _deviceAttempts[id] = new DeviceLoginAttempt { DeviceCode = response.DeviceCode, ExpiresAt = DateTime.UtcNow.AddSeconds(Math.Max(response.ExpiresIn, 60)) };

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

            TokenResponse? token = attempt.IssuedToken;
            if (token == null)
            {
                DevicePollResult result;
                try
                {
                    result = await _twitchOAuthClient.PollDeviceCodeAsync(attempt.DeviceCode, Scopes);
                }
                catch (Exception ex)
                {
                    // A network blip during polling says nothing about the
                    // login; pending keeps the UI polling.
                    _logger.LogWarning(ex, "twitch device login poll failed");
                    return ResponseHelper.OkStatus("pending");
                }

                if (result.Status == DevicePollStatus.Pending)
                {
                    return ResponseHelper.OkStatus("pending");
                }
                if (result.Status == DevicePollStatus.Failed)
                {
                    _deviceAttempts.TryRemove(id, out _);
                    return ResponseHelper.OkStatus("failed");
                }

                token = result.Token!;
                // The device code is single-use: cache the granted tokens so a
                // retried poll validates them instead of re-polling a consumed code.
                _deviceAttempts[id] = new DeviceLoginAttempt { DeviceCode = attempt.DeviceCode, ExpiresAt = attempt.ExpiresAt, IssuedToken = token };
            }

            TokenValidationResponse? validation;
            try
            {
                validation = await _twitchOAuthClient.ValidateTokenAsync(token.AccessToken);
            }
            catch (Exception ex)
            {
                // Transient failure - the attempt (with its cached token) stays
                // alive so the next poll retries validation instead of forcing
                // the user through a full re-login.
                _logger.LogWarning(ex, "twitch token validation failed after device login");
                return ResponseHelper.OkStatus("pending");
            }

            // The outcome is final; release the single-use device code.
            _deviceAttempts.TryRemove(id, out _);

            if (validation == null)
            {
                return ResponseHelper.OkStatus("failed");
            }

            // Persist the rotation details first, then publish the new token
            // through the token service last, so watchers never see a new
            // token alongside a stale expiry.
            if (!string.IsNullOrEmpty(token.RefreshToken))
            {
                _twitchRefreshTokenService.SetRefreshToken(token.RefreshToken);
            }
            _twitchTokenExpiresAtService.SetTokenExpiresAt(DateTime.UtcNow.AddSeconds(token.ExpiresIn).ToString("o"));
            _twitchUsernameService.SetUsername(validation.Login);

            _twitchTokenService.SetToken(token.AccessToken);

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