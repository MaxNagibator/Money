using Refit;

namespace Money.Api.Constracts.Auth
{
    public interface IAuthResource
    {
        [Post("/connect/token")]
        [Headers("Content-Type: application/x-www-form-urlencoded")]
        Task<AuthTokens> RefreshTokenAsync(
            [Body(BodySerializationMethod.UrlEncoded)] RefreshTokenInfo refreshTokenInfo,
            CancellationToken cancellationToken = default);

        [Post("/connect/token")]
        [Headers("Content-Type: application/x-www-form-urlencoded")]
        Task<AuthTokens> LoginAsync(
            [Body(BodySerializationMethod.UrlEncoded)] UserAuthInfo userAuthInfo,
            CancellationToken cancellationToken = default);
    }
}