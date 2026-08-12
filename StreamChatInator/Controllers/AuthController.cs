using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamChatInator.Apis;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Services;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;

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

        private readonly IConfiguration _config;
        private readonly DatabaseContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TwitchTokenService _tokenService;
        private readonly LanAccessService _lanAccess;

        public AuthController(IConfiguration config, DatabaseContext db, IHttpClientFactory httpClientFactory, TwitchTokenService tokenService, LanAccessService lanAccess)
        {
            _config = config;
            _db = db;
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _lanAccess = lanAccess;
        }

        /// <summary>
        /// Starts a Twitch device code login: asks Twitch for a code, stores the
        /// device code server-side, and returns everything the UI needs to show
        /// the user (verification URL + code) plus the polling id.
        /// </summary>
        [HttpGet("login")]
        public async Task<IActionResult> Login()
        {
            var httpClient = _httpClientFactory.CreateClient("twitch");
            var device = await Twitch.RequestDeviceCodeAsync(httpClient, ClientId, Scopes);
            if (device == null || string.IsNullOrEmpty(device.DeviceCode))
            {
                return StatusCode(502, new { error = "twitch_unavailable" });
            }

            PruneExpiredAttempts();

            var id = CreateRandom();
            _deviceAttempts[id] = (device.DeviceCode, DateTime.UtcNow.AddSeconds(Math.Max(device.ExpiresIn, 60)));

            return Ok(new
            {
                id,
                userCode = device.UserCode,
                verificationUri = device.VerificationUri,
                expiresIn = device.ExpiresIn,
                interval = device.Interval,
            });
        }

        /// <summary>
        /// Called by the UI to see whether the device login completed. Each call
        /// performs a single Twitch poll; the UI is expected to poll roughly every
        /// <c>interval</c> seconds. On success the tokens are persisted and the
        /// attempt is removed.
        /// </summary>
        [HttpGet("device-status")]
        public async Task<IActionResult> DeviceStatus(string? id)
        {
            if (string.IsNullOrEmpty(id) || !_deviceAttempts.TryGetValue(id, out var attempt))
            {
                return Ok(new { status = "expired" });
            }

            if (attempt.ExpiresAt < DateTime.UtcNow)
            {
                _deviceAttempts.TryRemove(id, out _);
                return Ok(new { status = "expired" });
            }

            var httpClient = _httpClientFactory.CreateClient("twitch");
            var result = await Twitch.PollDeviceCodeAsync(httpClient, ClientId, attempt.DeviceCode, Scopes);
            if (result.Status == Twitch.DevicePollStatus.Pending)
            {
                return Ok(new { status = "pending" });
            }
            if (result.Status == Twitch.DevicePollStatus.Failed)
            {
                _deviceAttempts.TryRemove(id, out _);
                return Ok(new { status = "failed" });
            }

            _deviceAttempts.TryRemove(id, out _);

            var token = result.Token!;
            var validation = await Twitch.ValidateTokenAsync(httpClient, token.AccessToken);
            if (validation == null || !string.Equals(validation.ClientId, ClientId, StringComparison.Ordinal))
            {
                return Ok(new { status = "failed" });
            }

            _db.SetSettingsValue(SettingValue.SettingOAuthToken, token.AccessToken);
            if (!string.IsNullOrEmpty(token.RefreshToken))
            {
                _db.SetSettingsValue(SettingValue.SettingOAuthRefreshToken, token.RefreshToken);
            }
            _db.SetSettingsValue(SettingValue.SettingOAuthTokenExpiresAt, DateTime.UtcNow.AddSeconds(token.ExpiresIn).ToString("o"));
            _db.SetSettingsValue(SettingValue.SettingUserName, validation.Login);
            _db.SaveChanges();

            _tokenService.SignalLogin();

            return Ok(new { status = "ok", username = validation.Login });
        }

        #region LAN access (PIN + session cookie)

        /// <summary>
        /// Whether this browser is allowed in. When Auth:Enabled=false this is
        /// always true, which is what lets the frontend skip the login screen
        /// entirely when gating is opted out.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new { authenticated = !_lanAccess.Enabled || User.Identity?.IsAuthenticated == true });
        }

        /// <summary>
        /// Validates the shared LAN PIN and issues a session cookie. A few bad
        /// attempts briefly lock out further tries to make a short PIN harder
        /// to brute-force over the network.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("pin-login")]
        public async Task<IActionResult> PinLogin([FromBody] PinLoginRequest request)
        {
            if (!_lanAccess.Enabled) return NotFound();

            // Lockout is per client IP so one device hammering the PIN doesn't
            // lock everyone else out of the UI.
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? null;

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

        public class PinLoginRequest
        {
            public required string Pin { get; set; }
        }

        #endregion

        private string ClientId => _config["Twitch:ClientId"] ?? Constants.TwitchAppClientId;

        private static string CreateRandom()
        {
            return Base64UrlEncode(RandomNumberGenerator.GetBytes(24));
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
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