using System.Text.Json.Serialization;

namespace Money.Web.Models.Charts.Config;

public sealed class ChartAxis
{
    [JsonPropertyName("stacked")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stacked { get; set; }

    [JsonPropertyName("beginAtZero")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? BeginAtZero { get; set; }

    [JsonPropertyName("grid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartGrid? Grid { get; set; }

    [JsonPropertyName("ticks")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartTicks? Ticks { get; set; }
}
