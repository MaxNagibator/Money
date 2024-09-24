using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace Money.Web.Services;

public class RefreshTokenHandler(RefreshTokenService refreshTokenService, ILocalStorageService localStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? absPath = request.RequestUri?.AbsolutePath;

        if (absPath != null && (absPath.Contains("token") || absPath.Contains("register")))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        await refreshTokenService.TryRefreshToken();

        string? token = await localStorage.GetItemAsync<string>("authToken", cancellationToken);

        if (string.IsNullOrEmpty(token) == false)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
