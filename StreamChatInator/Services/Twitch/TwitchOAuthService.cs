using StreamChatInator.Services.Twitch.Responses;
using System.Net.Http.Headers;
using System.Text.Json;

namespace StreamChatInator.Services.Twitch
{
    /// <summary>
    /// Twitch OAuth endpoints (id.twitch.tv): device-code login, token refresh
    /// and token validation. Helix API calls live in <see cref="TwitchApiService"/>.
    /// </summary>
    public class TwitchOAuthService
    {
        private const string TokenUrl = "https://id.twitch.tv/oauth2/token";
        private const string DeviceUrl = "https://id.twitch.tv/oauth2/device";
        private const string ValidateUrl = "https://id.twitch.tv/oauth2/validate";

        private readonly ConfigService _config;
        private readonly HttpClient _httpClient;

        public TwitchOAuthService(HttpClient httpClient, ConfigService config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<TokenResponse?> RefreshAccessTokenAsync(string refreshToken)
        {
            var content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("client_id",_config.TwitchClientId),
                new KeyValuePair<string, string>("grant_type","refresh_token"),
                new KeyValuePair<string, string>("refresh_token",refreshToken)
            ]);

            using var response = await _httpClient.PostAsync(TokenUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<TokenResponse>();
        }

        public async Task<TokenValidationResponse?> ValidateTokenAsync(string bearerToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, ValidateUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var validation = await response.Content.ReadFromJsonAsync<TokenValidationResponse>();
            if (!string.Equals(validation?.ClientId, _config.TwitchClientId, StringComparison.Ordinal))
            {
                return null;
            }
            return validation;
        }

        public async Task<DeviceCodeResponse?> RequestDeviceCodeAsync(string scopes)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _config.TwitchClientId,
                ["scopes"] = scopes,
            });

            using var response = await _httpClient.PostAsync(DeviceUrl, form);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<DeviceCodeResponse>();
        }

        public async Task<DevicePollResult> PollDeviceCodeAsync(string deviceCode, string scopes)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _config.TwitchClientId,
                ["device_code"] = deviceCode,
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                ["scopes"] = scopes,
            });

            using var response = await _httpClient.PostAsync(TokenUrl, form);

            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var token = JsonSerializer.Deserialize<TokenResponse>(body);
                if (token != null && !string.IsNullOrEmpty(token.AccessToken))
                {
                    return new DevicePollResult { Status = DevicePollStatus.Success, Token = token };
                }
                return new DevicePollResult { Status = DevicePollStatus.Failed, Message = body };
            }

            var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
            return messageResponse?.Message switch
            {
                "authorization_pending" or "slow_down" => new DevicePollResult { Status = DevicePollStatus.Pending, Message = messageResponse.Message },
                _ => new DevicePollResult { Status = DevicePollStatus.Failed, Message = messageResponse?.Message ?? await response.Content.ReadAsStringAsync() },
            };
        }
    }
}
