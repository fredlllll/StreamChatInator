using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamChatInator.Services
{
    public class TwitchApiService
    {
        private const string TokenUrl = "https://id.twitch.tv/oauth2/token";
        private const string DeviceUrl = "https://id.twitch.tv/oauth2/device";
        private const string ValidateUrl = "https://id.twitch.tv/oauth2/validate";
        private const string GlobalBadgesUrl = "https://api.twitch.tv/helix/chat/badges/global";
        private const string ChannelBadgesUrl = "https://api.twitch.tv/helix/chat/badges";

        private readonly IHttpClientFactory _httpClientFactory;

        public TwitchApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetHttpClient()
        {
            return _httpClientFactory.CreateClient("twitch");
        }

        public async Task<TwitchTokenResponse?> RefreshAccessTokenAsync(string clientId, string refreshToken)
        {
            var httpClient = GetHttpClient();

            var content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("client_id",clientId),
                new KeyValuePair<string,string>("grant_type","refresh_token"),
                new KeyValuePair<string,string>("refresh_token",refreshToken)
            ]);

            using var response = await httpClient.PostAsync(TokenUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<TwitchTokenResponse>();
        }

        public  async Task<TwitchTokenValidationResponse?> ValidateTokenAsync(string bearerToken)
        {
            var httpClient = GetHttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, ValidateUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            using var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<TwitchTokenValidationResponse>();
        }

        public async Task<TwitchDeviceCodeResponse?> RequestDeviceCodeAsync(string clientId, string scopes)
        {
            var httpClient = GetHttpClient();
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["scopes"] = scopes,
            });

            using var response = await httpClient.PostAsync(DeviceUrl, form);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<TwitchDeviceCodeResponse>();
        }

        public async Task<DevicePollResult> PollDeviceCodeAsync(string clientId, string deviceCode, string scopes)
        {
            var httpClient = GetHttpClient();
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["device_code"] = deviceCode,
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                ["scopes"] = scopes,
            });

            using var response = await httpClient.PostAsync(TokenUrl, form);
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var token = JsonSerializer.Deserialize<TwitchTokenResponse>(body);
                if (token != null && !string.IsNullOrEmpty(token.AccessToken))
                {
                    return new DevicePollResult { Status = DevicePollStatus.Success, Token = token };
                }
                return new DevicePollResult { Status = DevicePollStatus.Failed, Message = body };
            }

            var message = ParseMessage(body);
            return message switch
            {
                "authorization_pending" or "slow_down" => new DevicePollResult { Status = DevicePollStatus.Pending, Message = message },
                _ => new DevicePollResult { Status = DevicePollStatus.Failed, Message = message },
            };
        }

        public enum DevicePollStatus
        {
            Pending,
            Success,
            Failed,
        }

        public class DevicePollResult
        {
            public DevicePollStatus Status { get; init; }
            public TwitchTokenResponse? Token { get; init; }
            public string Message { get; init; } = string.Empty;
        }

        public static string ParseMessage(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("message", out var message))
                {
                    return message.GetString() ?? string.Empty;
                }
            }
            catch (JsonException)
            {
            }
            return body;
        }

        public async Task<List<TwitchBadgeSet>?> GetGlobalBadgesAsync(string clientId, string bearerToken)
        {
            var httpClient = GetHttpClient();
            return await GetBadgesAsync(clientId, bearerToken, GlobalBadgesUrl);
        }

        public async Task<List<TwitchBadgeSet>?> GetChannelBadgesAsync(string clientId, string bearerToken, string broadcasterId)
        {
            var httpClient = GetHttpClient();
            var url = $"{ChannelBadgesUrl}?broadcaster_id={Uri.EscapeDataString(broadcasterId)}";
            return await GetBadgesAsync(clientId, bearerToken, url);
        }

        private async Task<List<TwitchBadgeSet>?> GetBadgesAsync(string clientId, string bearerToken, string url)
        {
            var httpClient = GetHttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            request.Headers.Add("Client-Id", clientId);

            using var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var body = await response.Content.ReadFromJsonAsync<TwitchBadgesResponse>();
            return body?.Data;
        }

        public class TwitchBadgesResponse
        {
            [JsonPropertyName("data")]
            public List<TwitchBadgeSet> Data { get; set; } = new();
        }

        public class TwitchBadgeSet
        {
            [JsonPropertyName("set_id")]
            public string SetId { get; set; } = string.Empty;

            [JsonPropertyName("versions")]
            public List<TwitchBadgeVersion> Versions { get; set; } = new();
        }

        public class TwitchBadgeVersion
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("image_url_1x")]
            public string ImageUrl1x { get; set; } = string.Empty;

            [JsonPropertyName("image_url_2x")]
            public string ImageUrl2x { get; set; } = string.Empty;

            [JsonPropertyName("image_url_4x")]
            public string ImageUrl4x { get; set; } = string.Empty;

            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;

            [JsonPropertyName("click_action")]
            public string? ClickAction { get; set; }

            [JsonPropertyName("click_url")]
            public string? ClickUrl { get; set; }
        }

        public class TwitchDeviceCodeResponse
        {
            [JsonPropertyName("device_code")]
            public string DeviceCode { get; set; } = string.Empty;

            [JsonPropertyName("user_code")]
            public string UserCode { get; set; } = string.Empty;

            [JsonPropertyName("verification_uri")]
            public string VerificationUri { get; set; } = string.Empty;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("interval")]
            public int Interval { get; set; }
        }

        public class TwitchTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; } = string.Empty;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("scope")]
            public List<string> Scope { get; set; } = new();

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = string.Empty;
        }

        public class TwitchTokenValidationResponse
        {
            [JsonPropertyName("client_id")]
            public string ClientId { get; set; } = string.Empty;

            [JsonPropertyName("login")]
            public string Login { get; set; } = string.Empty;

            [JsonPropertyName("scopes")]
            public List<string> Scopes { get; set; } = new();

            [JsonPropertyName("user_id")]
            public string UserId { get; set; } = string.Empty;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }
    }
}
