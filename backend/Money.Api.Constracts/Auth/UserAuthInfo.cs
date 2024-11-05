using Refit;
using System.Text.Json.Serialization;

namespace Money.Api.Constracts.Auth
{
    public record UserAuthInfo(
        [property: JsonPropertyName("username"), AliasAs("username")] string Username,
        [property: JsonPropertyName("password"), AliasAs("password")] string Password,
        [property: JsonPropertyName("grant_type"), AliasAs("grant_type")] string GrantType = "password");

    public record RefreshTokenInfo(
        [property: JsonPropertyName("refresh_token"), AliasAs("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("grant_type"), AliasAs("grant_type")] string GrantType = "refresh_token");
}