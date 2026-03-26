using FintachartsAPI.DTOs;

namespace FintachartsAPI.Clients
{
    public interface IFintachartsApiClient
    {
        Task<(string AccessToken, int ExpiresIn)> GetTokenAsync(string username, string password);

        Task<InstrumentsResponseDto?> GetInstrumentsAsync(string token);

        Task<FintachartsHistoricalResponseDto?> GetHistoricalBarsAsync(string token, string instrumentId, string provider);
    }
}