using System.Text.Json.Serialization;

namespace FintachartsAPI.DTOs
{
    public record FintachartsHistoricalResponseDto
    (
        [property: JsonPropertyName("data")] List<FintachartsBarDto> Data
    );

    public record FintachartsBarDto
    (
        [property: JsonPropertyName("t")] DateTime Timestamp,
        [property: JsonPropertyName("c")] decimal ClosePrice
    );
}