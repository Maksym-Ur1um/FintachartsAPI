using FintachartsAPI.Clients;
using FintachartsAPI.Data;
using FintachartsAPI.Models;

namespace FintachartsAPI.Services
{
    public class FintachartsDataService : IFintachartsDataService
    {
        private readonly AppDbContext _dbContext;
        private readonly IFintachartsApiClient _apiClient;

        public FintachartsDataService(
            AppDbContext dbContext,
            IFintachartsApiClient apiClient)
        {
            _dbContext = dbContext;
            _apiClient = apiClient;
        }

        public async Task InitializeAssetsAsync()
        {
            var responseDto = await _apiClient.GetInstrumentsAsync();

            if (responseDto == null || responseDto.InstrumentData == null)
            {
                throw new InvalidOperationException("Failed to load instruments data from API.");
            }

            var existingIds = _dbContext.Assets.Select(a => a.FintachartsId).ToList();
            var newAssets = new List<Asset>();

            foreach (var instrument in responseDto.InstrumentData)
            {
                if (existingIds.Contains(instrument.Id)) continue;

                newAssets.Add(new Asset
                {
                    FintachartsId = instrument.Id,
                    Symbol = instrument.Symbol,
                    Kind = instrument.Kind,
                    Provider = instrument.Mappings.Keys.FirstOrDefault() ?? "Unknown"
                });
            }

            if (newAssets.Any())
            {
                _dbContext.Assets.AddRange(newAssets);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}