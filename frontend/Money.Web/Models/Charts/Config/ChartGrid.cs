using System.Text.Json.Serialization;

namespace Money.Web.Models.Charts.Config;

public sealed class ChartGrid
{
    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }
}