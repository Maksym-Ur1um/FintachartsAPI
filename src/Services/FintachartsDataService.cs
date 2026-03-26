using FintachartsAPI.Data;
using FintachartsAPI.DTOs;
using FintachartsAPI.Models;

namespace FintachartsAPI.Services
{
    public class FintachartsDataService : IFintachartsDataService
    {
        private readonly AppDbContext _dbContext;
        private readonly HttpClient _httpClient;
        private readonly IFintachartsAuthService _fintachartsAuthService;

        public FintachartsDataService(AppDbContext dbContext, HttpClient httpClient, IFintachartsAuthService fintachartsAuthService)
        {
            _dbContext = dbContext;
            _httpClient = httpClient;
            _fintachartsAuthService = fintachartsAuthService;
        }

        public async Task InitializeAssetsAsync()
        {
            string token = await _fintachartsAuthService.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("/api/instruments/v1/instruments");
            response.EnsureSuccessStatusCode();

            var responseDto = await response.Content.ReadFromJsonAsync<InstrumentsResponseDto>();
            if (responseDto == null || responseDto.InstrumentData == null)
            {
                throw new InvalidOperationException("Failed to load instruments data from API.");
            }

            var existingIds = _dbContext.Assets.Select(a => a.FintachartsId).ToList();
            var newAssets = new List<Asset>();

            foreach (var instrument in responseDto.InstrumentData)
            {
                if (existingIds.Contains(instrument.Id))
                    continue;
                var asset = new Asset
                {
                    FintachartsId = instrument.Id,
                    Symbol = instrument.Symbol,
                    Kind = instrument.Kind,
                    Provider = instrument.Mappings.Keys.FirstOrDefault() ?? "Unknown"
                };
                newAssets.Add(asset);
            }

            if (newAssets.Any())
            {
                _dbContext.Assets.AddRange(newAssets);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}