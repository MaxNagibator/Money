using Blazored.LocalStorage;

namespace Money.Web.Services;

public class AuthTokensStore
{
    public const string AccessTokenKey = "authToken";
    public const string RefreshTokenKey = "refreshToken";

    private readonly ILocalStorageService _localStorage;

    public AuthTokensStore(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public ValueTask<string?> GetAccessToken(CancellationToken cancellationToken = default)
    {
        return _localStorage.GetItemAsync<string>(AccessTokenKey, cancellationToken);
    }

    public ValueTask<string?> GetRefreshToken(CancellationToken cancellationToken = default)
    {
        return _localStorage.GetItemAsync<string>(RefreshTokenKey, cancellationToken);
    }

    public ValueTask SetAccessToken(string accessToken, CancellationToken cancellationToken = default)
    {
        return _localStorage.SetItemAsync<string>(AccessTokenKey, accessToken, cancellationToken);
    }

    public ValueTask SetRefreshToken(string refreshToken, CancellationToken cancellationToken = default)
    {
        return _localStorage.SetItemAsync<string>(RefreshTokenKey, refreshToken, cancellationToken);
    }

    public ValueTask Clear(CancellationToken cancellationToken = default)
    {
        return _localStorage.RemoveItemsAsync([AccessTokenKey, RefreshTokenKey], cancellationToken);
    }
}