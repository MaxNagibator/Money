using CSharpFunctionalExtensions;

namespace Money.Web.Services.Authentication;

public class RefreshTokenService(AuthenticationStateProvider authProvider, AuthenticationService authService)
{
    public async Task<Result<string>> TryRefreshToken()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == false)
        {
            return Result.Success(string.Empty);
        }

        var exp = user.FindFirst(c => c.Type.Equals("exp", StringComparison.Ordinal))?.Value;

        if (string.IsNullOrWhiteSpace(exp))
        {
            return Result.Success(string.Empty);
        }

        var expTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp));
        var timeUtc = DateTime.UtcNow;
        var diff = expTime - timeUtc;

        if (diff.TotalMinutes <= 2)
        {
            var refreshResult = await authService.RefreshTokenAsync();

            if (refreshResult.IsSuccess)
            {
                await ((AuthStateProvider)authProvider).NotifyUserAuthentication();
            }

            return refreshResult;
        }

        return Result.Success(string.Empty);
    }
}
