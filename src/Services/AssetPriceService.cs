using FintachartsAPI.Clients;
using FintachartsAPI.Data;
using FintachartsAPI.DTOs;
using FintachartsAPI.Models;
using FintachartsAPI.State;
using Microsoft.EntityFrameworkCore;

namespace FintachartsAPI.Services
{
    public class AssetPriceService : IAssetPriceService
    {
        private readonly AppDbContext _appDbContext;
        private readonly MarketStateCache _marketStatceCache;
        private readonly IFintachartsApiClient _apiClient;
        private readonly IFintachartsAuthService _authService;
        private readonly ILogger<AssetPriceService> _logger;

        public AssetPriceService(
            AppDbContext appDbContext,
            MarketStateCache marketStatceCache,
            IFintachartsApiClient apiClient,
            IFintachartsAuthService authService,
            ILogger<AssetPriceService> logger)
        {
            _appDbContext = appDbContext;
            _marketStatceCache = marketStatceCache;
            _apiClient = apiClient;
            _authService = authService;
            _logger = logger;
        }

        public async Task<List<AssetPriceResponseDto>> GetPricesAsync(string[] assets)
        {
            _logger.LogInformation("Getting prices for assets from cache...");

            var cacheData = _marketStatceCache.GetAssetsData(assets);
            var results = cacheData.Select(kvp => new AssetPriceResponseDto(
                kvp.Key, kvp.Value.Price, kvp.Value.LastUpdate)).ToList();

            var missingSymbols = assets.Where(a => !cacheData.ContainsKey(a)).ToList();

            if (missingSymbols.Any())
            {
                var dbAssets = await _appDbContext.Assets
                    .Where(a => missingSymbols.Contains(a.Symbol))
                    .ToListAsync();

                if (dbAssets.Any())
                {
                    _logger.LogInformation($"Fetching prices for: {dbAssets.Count} assets that were not in cache...");
                    var token = await _authService.GetTokenAsync();

                    using var semaphore = new SemaphoreSlim(5);
                    var fetchTasks = new List<Task<AssetPriceResponseDto?>>();

                    foreach (var asset in dbAssets)
                    {
                        fetchTasks.Add(FetchAssetPriceAsync(asset, token, semaphore));
                    }
                    var fetchedAssets = await Task.WhenAll(fetchTasks);
                    results.AddRange(fetchedAssets.Where(a => a != null)!);
                }
            }
            return results;
        }

        private async Task<AssetPriceResponseDto?> FetchAssetPriceAsync(
            Asset asset, string token, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();

            try
            {
                var historicalData = await _apiClient.GetHistoricalBarsAsync(
                    token, asset.FintachartsId.ToString(), asset.Provider);

                var lastBar = historicalData?.Data?.FirstOrDefault();

                if (lastBar != null)
                {
                    _marketStatceCache.UpdatePrice(asset.Symbol, lastBar.ClosePrice, lastBar.Timestamp);
                    return new AssetPriceResponseDto(asset.Symbol, lastBar.ClosePrice, lastBar.Timestamp);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch {asset.Symbol}");
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}