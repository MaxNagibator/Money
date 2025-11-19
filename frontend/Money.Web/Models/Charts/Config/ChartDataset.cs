using System.Text.Json.Serialization;

namespace Money.Web.Models.Charts.Config;

public sealed class ChartDataset
{
    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    [JsonPropertyName("backgroundColor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? BackgroundColor { get; set; }

    [JsonPropertyName("data")]
    public List<decimal?> Data { get; set; } = [];

    public void Add(decimal? value)
    {
        Data.Add(value);
    }
}