using FintachartsAPI.DTOs;
using System.Text.Json.Serialization;

namespace FintachartsAPI.Clients
{
    public class FintachartsApiClient : IFintachartsApiClient
    {
        private readonly HttpClient _httpClient;

        public FintachartsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(string AccessToken, int ExpiresIn)> GetTokenAsync(string username, string password)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "grant_type", "password" },
                { "client_id", "app-cli" },
                { "username", username },
                { "password", password }
            });

            var response = await _httpClient.PostAsync("/identity/realms/fintatech/protocol/openid-connect/token", content);
            response.EnsureSuccessStatusCode();

            var responseDto = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
            if (responseDto == null || string.IsNullOrEmpty(responseDto.AccessToken))
            {
                throw new InvalidOperationException("Failed to retrieve access token from API.");
            }

            return (responseDto.AccessToken, responseDto.ExpiresIn);
        }

        public async Task<InstrumentsResponseDto?> GetInstrumentsAsync(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/instruments/v1/instruments");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<InstrumentsResponseDto>();
        }

        public async Task<FintachartsHistoricalResponseDto?> GetHistoricalBarsAsync(string token, string instrumentId, string provider)
        {
            string url = $"/api/bars/v1/bars/count-back?instrumentId={instrumentId}&provider={provider}&interval=1&periodicity=minute&barsCount=1";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<FintachartsHistoricalResponseDto>();
        }

        private record TokenResponseDto(
            [property: JsonPropertyName("access_token")] string AccessToken,
            [property: JsonPropertyName("expires_in")] int ExpiresIn
        );
    }
}