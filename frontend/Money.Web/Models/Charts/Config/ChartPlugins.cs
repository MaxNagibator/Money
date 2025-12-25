using System.Text.Json.Serialization;

namespace Money.Web.Models.Charts.Config;

public sealed class ChartPlugins
{
    [JsonPropertyName("legend")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChartLegend? Legend { get; set; }
}