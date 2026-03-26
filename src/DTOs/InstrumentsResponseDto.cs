using System.Text.Json.Serialization;

namespace FintachartsAPI.DTOs
{
    public record InstrumentsResponseDto(
        [property: JsonPropertyName("data")] List<InstrumentDataDto> InstrumentData
    );
}