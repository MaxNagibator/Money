using Blazored.LocalStorage;

namespace Money.Web.Services.Authentication;

public class RefreshTokenHandler(RefreshTokenService refreshTokenService, ILocalStorageService localStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var absolutePath = request.RequestUri?.AbsolutePath;

        if (absolutePath != null && (absolutePath.Contains("token") || absolutePath.Contains("register")))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var result = await refreshTokenService.TryRefreshToken();

        if (result.IsSuccess == false)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = await localStorage.GetItemAsync<string>("authToken", cancellationToken);

        if (string.IsNullOrEmpty(token) == false)
        {
            request.Headers.Authorization = new("bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
