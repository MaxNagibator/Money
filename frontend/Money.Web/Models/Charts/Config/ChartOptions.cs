using System.Text.Json.Serialization;

namespace Money.Web.Models.Charts.Config;

public sealed class ChartOptions
{
    [JsonPropertyName("responsive")]
    public bool Responsive { get; set; } = true;

    [JsonPropertyName("scales")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, ChartAxis>? Scales { get; set; }

    [JsonPropertyName("plugins")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartPlugins? Plugins { get; set; }
}