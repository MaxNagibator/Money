using System.Text.Json.Serialization;

namespace Money.Api.Constracts
{
    public sealed class ProblemDetails
    {
        [JsonPropertyName("title")]
        public required string Title { get; init; }

        [JsonPropertyName("status")]
        public required int Status { get; init; }
    }
}