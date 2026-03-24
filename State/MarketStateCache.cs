using System.Collections.Concurrent;

namespace FintachartsAPI.State
{
    public class MarketStateCache
    {
        public record AssetPriceData(
            decimal Price,
            DateTime LastUpdate
            );
        private readonly ConcurrentDictionary<string, AssetPriceData> _assetPrices = new();

        public void UpdatePrice(string symbol, decimal price, DateTime lastUpdate)
        {
            _assetPrices[symbol] = new AssetPriceData(price, lastUpdate);
        }

        public AssetPriceData? GetAssetData(string symbol)
        {
            _assetPrices.TryGetValue(symbol, out AssetPriceData? assertPriceData);
            return assertPriceData;
        }

        public Dictionary<string, AssetPriceData> GetAssetsData(IEnumerable<string> symbols)
        {
            Dictionary<string, AssetPriceData> assetsData = new();
            foreach (var symbol in symbols)
            {
                var assetData = GetAssetData(symbol);
                if (assetData == null)
                    continue;
                assetsData.Add(symbol, assetData);
            }
            return assetsData;
        }
    }
}