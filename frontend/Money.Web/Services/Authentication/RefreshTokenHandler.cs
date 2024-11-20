using Blazored.LocalStorage;
using CSharpFunctionalExtensions;
using System.Net.Http.Headers;

namespace Money.Web.Services.Authentication;

public class RefreshTokenHandler(RefreshTokenService refreshTokenService, ILocalStorageService localStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? absolutePath = request.RequestUri?.AbsolutePath;

        if (absolutePath != null && (absolutePath.Contains("token") || absolutePath.Contains("register")))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        Result<string> result = await refreshTokenService.TryRefreshToken();

        if (result.IsSuccess == false)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        string? token = await localStorage.GetItemAsync<string>("authToken", cancellationToken);

        if (string.IsNullOrEmpty(token) == false)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
