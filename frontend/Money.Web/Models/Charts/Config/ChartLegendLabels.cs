using System.Text.Json.Serialization;

namespace Money.Web.Models.Charts.Config;

public sealed class ChartLegendLabels
{
    [JsonPropertyName("boxWidth")]
    public int BoxWidth { get; set; }

    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }
}