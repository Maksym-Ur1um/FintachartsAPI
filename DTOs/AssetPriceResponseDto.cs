namespace FintachartsAPI.DTOs
{
    public record AssetPriceResponseDto(
        string Symbol,
        decimal Price,
        DateTime LastUpdate
    );
}