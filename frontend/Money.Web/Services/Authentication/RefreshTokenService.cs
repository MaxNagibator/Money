using CSharpFunctionalExtensions;

namespace Money.Web.Services.Authentication;

public class RefreshTokenService(AuthenticationStateProvider authProvider, AuthenticationService authService)
{
    public async Task<Result<string>> TryRefreshToken()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var exp = user.FindFirst(c => c.Type.Equals("exp"))?.Value;

        if (string.IsNullOrWhiteSpace(exp))
        {
            return string.Empty;
        }

        var expTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp));

        var timeUtc = DateTime.UtcNow;

        var diff = expTime - timeUtc;

        if (diff.TotalMinutes <= 2)
        {
            return await authService.RefreshTokenAsync();
        }

        return string.Empty;
    }
}
