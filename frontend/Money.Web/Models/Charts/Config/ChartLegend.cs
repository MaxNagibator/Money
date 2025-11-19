using System.Text.Json.Serialization;

namespace Money.Web.Models.Charts.Config;

public sealed class ChartLegend
{
    [JsonPropertyName("display")]
    public bool Display { get; set; } = true;

    [JsonPropertyName("position")]
    public string Position { get; set; } = "top";

    [JsonPropertyName("labels")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartLegendLabels? Labels { get; set; }
}