using Blazored.LocalStorage;

namespace Money.Web.Services.Authentication;

public sealed class RefreshTokenService(
    AuthenticationService authService,
    ILocalStorageService localStorage) : IDisposable
{
    private static readonly TimeSpan RefreshThreshold = TimeSpan.FromMinutes(2);
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public void Dispose()
    {
        _semaphore.Dispose();
    }

    public async Task<Result<string>> TryRefreshToken()
    {
        var accessToken = await localStorage.GetItemAsync<string>(AuthenticationService.AccessTokenKey);

        if (string.IsNullOrWhiteSpace(accessToken) || !NeedsRefresh(accessToken))
        {
            return Result.Success(string.Empty);
        }

        await _semaphore.WaitAsync();

        try
        {
            var currentToken = await localStorage.GetItemAsync<string>(AuthenticationService.AccessTokenKey);

            if (string.IsNullOrWhiteSpace(currentToken))
            {
                return Result.Success(string.Empty);
            }

            if (!NeedsRefresh(currentToken))
            {
                return Result.Success(currentToken);
            }

            return await authService.RefreshTokenAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static bool NeedsRefresh(string accessToken)
    {
        var exp = JwtParser.TryReadExp(accessToken);

        if (exp == null)
        {
            return false;
        }

        var expTime = DateTimeOffset.FromUnixTimeSeconds(exp.Value);
        var remaining = expTime - DateTimeOffset.UtcNow;
        return remaining <= RefreshThreshold;
    }
}
