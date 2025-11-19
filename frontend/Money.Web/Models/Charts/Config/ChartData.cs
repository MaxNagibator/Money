using System.Text.Json.Serialization;

namespace Money.Web.Models.Charts.Config;

public sealed class ChartData
{
    [JsonPropertyName("labels")]
    public List<string> Labels { get; set; } = [];

    [JsonPropertyName("datasets")]
    public List<ChartDataset> Datasets { get; set; } = [];
}