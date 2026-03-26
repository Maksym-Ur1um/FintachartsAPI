using FintachartsAPI.Data;
using FintachartsAPI.DTOs;
using FintachartsAPI.State;
using Microsoft.EntityFrameworkCore;

namespace FintachartsAPI.Services
{
    public class AssetPriceService : IAssetPriceService
    {
        private readonly AppDbContext _appDbContext;
        private readonly MarketStateCache _marketStatceCache;
        private readonly HttpClient _httpClient;
        private readonly IFintachartsAuthService _authService;
        private readonly ILogger<AssetPriceService> _logger;

        public AssetPriceService(AppDbContext appDbContext, MarketStateCache marketStatceCache,
            HttpClient httpClient, IFintachartsAuthService authService, ILogger<AssetPriceService> logger)
        {
            _appDbContext = appDbContext;
            _marketStatceCache = marketStatceCache;
            _httpClient = httpClient;
            _authService = authService;
            _logger = logger;
        }

        public async Task<List<AssetPriceResponseDto>> GetPricesAsync(string[] assets)
        {
            _logger.LogInformation("Getting prices for assets from cache...");

            var cacheData = _marketStatceCache.GetAssetsData(assets);

            var results = cacheData.Select(kvp => new AssetPriceResponseDto(
                kvp.Key, kvp.Value.Price, kvp.Value.LastUpdate)).ToList();

            var missingSymbols = assets.Where(a => !cacheData.ContainsKey(a));

            if (missingSymbols.Any())
            {
                var dbAssets = await _appDbContext.Assets.Where(a => missingSymbols.Contains(a.Symbol))
                    .ToListAsync();
                if (dbAssets.Any())
                {
                    _logger.LogInformation($"Fetching prices for: {dbAssets.Count} assets that were not in cache...");

                    var token = await _authService.GetTokenAsync();
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    foreach (var asset in dbAssets)
                    {
                        string url = $"/api/bars/v1/bars/count-back?instrumentId={asset.FintachartsId}&provider={asset.Provider}&interval=1&periodicity=minute&barsCount=1";

                        try
                        {
                            var response = await _httpClient.GetAsync(url);
                            if (response.IsSuccessStatusCode)
                            {
                                var historicalData =
                                    await response.Content.ReadFromJsonAsync<FintachartsHistoricalResponseDto>();
                                var lastBar = historicalData?.Data?.FirstOrDefault();

                                if (lastBar != null)
                                {
                                    _marketStatceCache.UpdatePrice(asset.Symbol, lastBar.ClosePrice, lastBar.Timestamp);
                                    results.Add(new AssetPriceResponseDto(asset.Symbol, lastBar.ClosePrice, lastBar.Timestamp));
                                }
                            }
                            else
                            {
                                _logger.LogError($"Failed to fetch {asset.Symbol}. Status code: {response.StatusCode}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to fetch {asset.Symbol}");
                            continue;
                        }
                    }
                }
            }
            return results;
        }
    }
}