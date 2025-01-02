using System.Text.Json.Serialization;

namespace Money.ApiClient;

public class ProblemDetails(string title, int status)
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = title;

    [JsonPropertyName("status")]
    public int Status { get; init; } = status;
}
