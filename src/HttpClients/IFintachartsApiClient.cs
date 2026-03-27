using FintachartsAPI.DTOs;

namespace FintachartsAPI.Clients
{
    public interface IFintachartsApiClient
    {
        Task<(string AccessToken, int ExpiresIn)> GetTokenAsync(string username, string password);

        Task<InstrumentsResponseDto?> GetInstrumentsAsync();

        Task<FintachartsHistoricalResponseDto?> GetHistoricalBarsAsync(string instrumentId, string provider);
    }
}