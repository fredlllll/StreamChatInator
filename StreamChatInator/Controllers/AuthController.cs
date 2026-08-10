using Microsoft.AspNetCore.Mvc;
using StreamChatInator.Apis;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace StreamChatInator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // PKCE login attempts, keyed by the random state we include in the
        // authorize URL. Each entry is single-use (removed when the callback
        // redeems the code) and short-lived, so concurrent logins don't clobber
        // each other and an in-flight login dies cleanly if the server restarts.
        private static readonly ConcurrentDictionary<string, (string Verifier, DateTime ExpiresAt)> _authAttempts = new();

        private static readonly TimeSpan AuthAttemptLifetime = TimeSpan.FromMinutes(10);

        private readonly IConfiguration _config;
        private readonly IServiceProvider _services;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IConfiguration config, IServiceProvider services, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _services = services;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var state = CreateRandom();
            var verifier = CreateRandom();
            var challenge = Base64UrlEncode(SHA256.HashData(Encoding.UTF8.GetBytes(verifier)));

            _authAttempts[state] = (verifier, DateTime.UtcNow + AuthAttemptLifetime);

            var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
            query.Add("client_id", ClientId);
            query.Add("redirect_uri", RedirectUri);
            query.Add("response_type", "code");
            query.Add("state", state);
            query.Add("scope", "chat:edit chat:read");
            query.Add("code_challenge", challenge);
            query.Add("code_challenge_method", "S256");
            string authUrl = $"https://id.twitch.tv/oauth2/authorize?{query}";

            return Redirect(authUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string? code, string? state, string? error)
        {
            // User denied the request, or Twitch didn't hand back everything we need.
            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return Redirect("/?auth=failed");
            }

            // Redeem for the access token server-side. The entry is single-use,
            // so a replay of the callback (or a callback for a login we never
            // started) fails here.
            if (!_authAttempts.TryRemove(state, out var attempt) || attempt.ExpiresAt < DateTime.UtcNow)
            {
                return Redirect("/?auth=failed");
            }

            var httpClient = _httpClientFactory.CreateClient("twitch");
            var token = await Twitch.ExchangeCodeAsync(httpClient, ClientId, code, attempt.Verifier, RedirectUri);
            if (token == null || string.IsNullOrEmpty(token.AccessToken))
            {
                return Redirect("/?auth=failed");
            }

            var validation = await Twitch.ValidateTokenAsync(httpClient, token.AccessToken);
            if (validation == null || !string.Equals(validation.ClientId, ClientId, StringComparison.Ordinal))
            {
                return Redirect("/?auth=failed");
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

            return Redirect("/");
        }

        private string ClientId => _config["Twitch:ClientId"] ?? Constants.TwitchAppClientId;

        private string RedirectUri => _config["Twitch:RedirectUri"] ?? $"{Request.Scheme}://{Request.Host}/api/auth/callback";

        private static string CreateRandom()
        {
            return Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}