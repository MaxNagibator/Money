using System.Text.Json.Serialization;

namespace Money.Api.Constracts.Auth
{
    public record AuthTokens(
        [property: JsonPropertyName("access_token")]
        string AccessToken,
        [property: JsonPropertyName("token_type")]
        string TokenType,
        [property: JsonPropertyName("expires_in")]
        int ExpiresIn,
        [property: JsonPropertyName("scope")]
        string Scope,
        [property: JsonPropertyName("refresh_token")]
        string RefreshToken);
}