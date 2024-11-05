using CSharpFunctionalExtensions;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Money.Web.Services;

public class RefreshTokenHandler : DelegatingHandler
{
    private readonly AuthTokensStore _tokensStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public RefreshTokenHandler(AuthTokensStore tokensStore, IServiceProvider serviceProvider, AuthenticationStateProvider authenticationStateProvider)
    {
        _tokensStore = tokensStore;
        _serviceProvider = serviceProvider;
        _authenticationStateProvider = authenticationStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? absPath = request.RequestUri?.AbsolutePath;

        if (absPath != null && (absPath.Contains("token") || absPath.Contains("register")))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        Result refreshTokenResult = await HasRefreshTokenExpired();

        if (refreshTokenResult.IsFailure)
        {
            AuthenticationService authService = _serviceProvider.GetRequiredService<AuthenticationService>();
            _ = await authService.RefreshTokenAsync();
        }

        string? token = await _tokensStore.GetAccessToken();

        if (string.IsNullOrEmpty(token) == false)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<Result> HasRefreshTokenExpired()
    {
        AuthenticationState authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal user = authState.User;

        string? exp = user.FindFirst(c => c.Type.Equals("exp"))?.Value;

        if (string.IsNullOrWhiteSpace(exp))
        {
            return Result.Failure("Токен не найден");
        }

        DateTimeOffset expTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp));

        DateTime timeUtc = DateTime.UtcNow;

        TimeSpan diff = expTime - timeUtc;

        return Result.SuccessIf(diff.TotalMinutes <= 2, "Токен истек");
    }
}
