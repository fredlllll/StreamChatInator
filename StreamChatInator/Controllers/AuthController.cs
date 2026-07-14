using Microsoft.AspNetCore.Mvc;
using StreamChatInator.Apis;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamChatInator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _services;
        static string lastOAuthState = Guid.NewGuid().ToString();

        // Inject your EF6 DbContext Factory or Token Manager here as well

        public AuthController(IConfiguration config, IServiceProvider services)
        {
            _config = config;
            _services = services;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            lastOAuthState = Guid.NewGuid().ToString();

            queryString.Add("client_id", Constants.TwitchAppClientId);
            queryString.Add("redirect_uri", "http://localhost:17455/api/auth/callback");
            queryString.Add("response_type", "token");
            queryString.Add("state", lastOAuthState);
            queryString.Add("scope", "chat:edit chat:read");
            string authUrl = $"https://id.twitch.tv/oauth2/authorize?{queryString}";

            return Redirect(authUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            var html = """
            <!DOCTYPE html>
            <html>
            <head>
                <title>Twitch Authentication</title>
                <style>
                    body { font-family: sans-serif; text-align: center; margin-top: 50px; background: #0e0e10; color: #efeff1; }
                </style>
            </head>
            <body>
                <h2 id='status'>Finalizing authentication...</h2>
                <script>
                    async function processTwitchToken() {
                        // 1. Extract the token from the URL fragment
                        const hash = window.location.hash.substring(1);
                        if (!hash) {
                            document.getElementById('status').innerText = 'Authentication failed: No token found.';
                            return;
                        }

                        const params = new URLSearchParams(hash);
                        const token = params.get('access_token');
                        const state = params.get('state');

                        if (token) {
                            try {
                                // 2. POST the token securely to your backend
                                const response = await fetch('/api/auth/settoken', {
                                    method: 'POST',
                                    headers: {
                                        'Content-Type': 'application/json'
                                    },
                                    body: JSON.stringify({
                                    token: token,
                                    state: state
                                    })
                                });

                                if (response.ok) {
                                    // 3. Redirect the user back to the main dashboard UI
                                    window.location.href = '/'; 
                                } else {
                                    document.getElementById('status').innerText = 'Failed to save token to server.';
                                }
                            } catch (err) {
                                document.getElementById('status').innerText = 'Connection error.';
                            }
                        }
                    }
                
                    // Run immediately on load
                    processTwitchToken();
                </script>
            </body>
            </html>                
            """;

            return Content(html, "text/html");
        }

        [HttpPost("settoken")]
        public async Task<IActionResult> SetToken([FromBody] TokenPayload payload)
        {
            if (string.IsNullOrEmpty(payload?.Token))
            {
                return BadRequest("Token is missing.");
            }
            if (payload.State != lastOAuthState)
            {
                return BadRequest("Invalid state parameter.");
            }

            var validation = await Twitch.ValidateTokenAsync(payload.Token);
            if (validation == null)
            {
                return BadRequest("Failed to validate token.");
            }

            var db = _services.GetRequiredService<DatabaseContext>();
            db.SetSettingsValue(SettingValue.SettingOAuthToken, payload.Token);
            db.SetSettingsValue(SettingValue.SettingUserName, validation.Login);

            return Ok();
        }

        public class TokenPayload
        {
            public string Token { get; set; } = string.Empty;
            public string State { get; set; } = string.Empty;
        }
    }
}
