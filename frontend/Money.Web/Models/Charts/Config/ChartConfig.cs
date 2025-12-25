using System.Text.Json.Serialization;

namespace Money.Web.Models.Charts.Config;

public sealed class ChartConfig
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("data")]
    public ChartData Data { get; set; } = new();

    [JsonPropertyName("options")]
    public ChartOptions Options { get; set; } = new();
}
