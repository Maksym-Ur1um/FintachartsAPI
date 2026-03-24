using FintachartsAPI.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FintachartsAPI.Services
{
    public class FintachartsAuthService : IFintachartsAuthService
    {
        private const string GrantType = "password";
        private const string ClientId = "app-cli";

        private readonly HttpClient _httpClient;
        private readonly IOptions<FintachartsOptions> _fintachartsOptions;
        private readonly IMemoryCache _memoryCache;

        public FintachartsAuthService(HttpClient httpClient, IOptions<FintachartsOptions> fintachartsOptions,
            IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _fintachartsOptions = fintachartsOptions;
            _memoryCache = memoryCache;
        }

        public async Task<string> GetTokenAsync()
        {
            if (_memoryCache.TryGetValue("FintaToken", out string? cachedToken))
            {
                return cachedToken!;
            }
            var formUrlEncodedContent = new Dictionary<string, string>()
            {
                { "grant_type", GrantType },
                { "client_id", ClientId },
                { "username", _fintachartsOptions.Value.UserName },
                { "password", _fintachartsOptions.Value.Password}
            };

            var content = new FormUrlEncodedContent(formUrlEncodedContent);
            string url = "/identity/realms/fintatech/protocol/openid-connect/token";

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<TokenResponseDto>(responseString);
            if (responseJson == null || String.IsNullOrEmpty(responseJson.AccessToken))
            {
                throw new InvalidOperationException("Failed to retrieve or deserialize the access token from Fintacharts API.");
            }
            var expiresIn = DateTimeOffset.UtcNow.AddSeconds(responseJson.ExpiresIn - 30);

            _memoryCache.Set("FintaToken", responseJson.AccessToken, expiresIn);

            return responseJson.AccessToken;
        }

        private record TokenResponseDto(
            [property: JsonPropertyName("access_token")] string AccessToken,
            [property: JsonPropertyName("expires_in")] int ExpiresIn
        );
    }
}