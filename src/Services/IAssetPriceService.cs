using FintachartsAPI.DTOs;

namespace FintachartsAPI.Services
{
    public interface IAssetPriceService
    {
        Task<List<AssetPriceResponseDto>> GetPricesAsync(string[] assets);
    }
}