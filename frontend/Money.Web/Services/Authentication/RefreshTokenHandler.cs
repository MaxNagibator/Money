using Blazored.LocalStorage;

namespace Money.Web.Services.Authentication;

public class RefreshTokenHandler(RefreshTokenService refreshTokenService, ILocalStorageService localStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var absolutePath = request.RequestUri?.AbsolutePath;

        if (absolutePath != null
            && (absolutePath.Contains("token", StringComparison.InvariantCultureIgnoreCase)
                || absolutePath.Contains("register", StringComparison.InvariantCultureIgnoreCase)))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var result = await refreshTokenService.TryRefreshToken();

        if (result.IsSuccess && string.IsNullOrEmpty(result.Value) == false)
        {
            request.Headers.Authorization = new("Bearer", result.Value);
        }
        else
        {
            var existingToken = await localStorage.GetItemAsync<string>("authToken", cancellationToken);

            if (string.IsNullOrEmpty(existingToken) == false)
            {
                request.Headers.Authorization = new("Bearer", existingToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
