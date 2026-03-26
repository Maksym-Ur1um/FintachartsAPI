using FintachartsAPI.Clients;
using FintachartsAPI.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FintachartsAPI.Services
{
    public class FintachartsAuthService : IFintachartsAuthService
    {
        private readonly IFintachartsApiClient _apiClient;
        private readonly IOptions<FintachartsOptions> _fintachartsOptions;
        private readonly IMemoryCache _memoryCache;

        public FintachartsAuthService(
            IFintachartsApiClient apiClient,
            IOptions<FintachartsOptions> fintachartsOptions,
            IMemoryCache memoryCache)
        {
            _apiClient = apiClient;
            _fintachartsOptions = fintachartsOptions;
            _memoryCache = memoryCache;
        }

        public async Task<string> GetTokenAsync()
        {
            if (_memoryCache.TryGetValue("FintaToken", out string? cachedToken))
            {
                return cachedToken!;
            }

            var tokenData = await _apiClient.GetTokenAsync(
                _fintachartsOptions.Value.UserName,
                _fintachartsOptions.Value.Password);

            var expiresIn = DateTimeOffset.UtcNow.AddSeconds(tokenData.ExpiresIn - 30);
            _memoryCache.Set("FintaToken", tokenData.AccessToken, expiresIn);

            return tokenData.AccessToken;
        }
    }
}