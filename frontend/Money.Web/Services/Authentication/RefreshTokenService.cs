using CSharpFunctionalExtensions;
using System.Security.Claims;

namespace Money.Web.Services.Authentication;

public class RefreshTokenService(AuthenticationStateProvider authProvider, AuthenticationService authService)
{
    public async Task<Result<string>> TryRefreshToken()
    {
        AuthenticationState authState = await authProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal user = authState.User;

        string? exp = user.FindFirst(c => c.Type.Equals("exp"))?.Value;

        if (string.IsNullOrWhiteSpace(exp))
        {
            return string.Empty;
        }

        DateTimeOffset expTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp));

        DateTime timeUtc = DateTime.UtcNow;

        TimeSpan diff = expTime - timeUtc;

        if (diff.TotalMinutes <= 2)
        {
            return await authService.RefreshTokenAsync();
        }

        return string.Empty;
    }
}
