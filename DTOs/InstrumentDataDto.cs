using System.Text.Json;
using System.Text.Json.Serialization;

namespace FintachartsAPI.DTOs
{
    public record InstrumentDataDto(
        [property: JsonPropertyName("symbol")] Guid Id,
        [property: JsonPropertyName("symbol")] string Symbol,
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("mappings")] Dictionary<string, JsonElement> Mappings
    );
}