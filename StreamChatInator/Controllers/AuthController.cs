using Microsoft.AspNetCore.Mvc;
using StreamChatInator.Apis;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using System.Collections.Concurrent;
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
        private readonly IServiceProvider _services;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IConfiguration config, IServiceProvider services, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _services = services;
            _httpClientFactory = httpClientFactory;
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

            var db = _services.GetRequiredService<DatabaseContext>();
            db.SetSettingsValue(SettingValue.SettingOAuthToken, token.AccessToken);
            if (!string.IsNullOrEmpty(token.RefreshToken))
            {
                db.SetSettingsValue(SettingValue.SettingOAuthRefreshToken, token.RefreshToken);
            }
            db.SetSettingsValue(SettingValue.SettingOAuthTokenExpiresAt, DateTime.UtcNow.AddSeconds(token.ExpiresIn).ToString("o"));
            db.SetSettingsValue(SettingValue.SettingUserName, validation.Login);
            db.SaveChanges();

            return Ok(new { status = "ok", username = validation.Login });
        }

        private string ClientId => _config["Twitch:ClientId"] ?? Constants.TwitchAppClientId;

        private static string CreateRandom()
        {
            return Base64UrlEncode(RandomNumberGenerator.GetBytes(24));
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}