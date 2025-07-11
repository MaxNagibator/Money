using Blazored.LocalStorage;
using CSharpFunctionalExtensions;
using Money.ApiClient;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using AuthData = Money.Web.Models.AuthData;

namespace Money.Web.Services.Authentication;

public class JwtParser(HttpClient client, ILocalStorageService localStorage)
{
    public async Task<ClaimsPrincipal?> ValidateJwt(string token)
    {
        var claimsDictionary = await ParseJwt(token);

        if (claimsDictionary == null)
        {
            return null;
        }

        List<Claim> claims = [];

        foreach (var (key, value) in claimsDictionary)
        {
            var claimType = key switch
            {
                "sub" => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                "name" => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                "email" => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                _ => key,
            };

            claims.Add(new(claimType, value.ToString() ?? string.Empty));
        }

        var claimsIdentity = new ClaimsIdentity(claims, "jwt");

        return new(claimsIdentity);
    }

    private async Task<Dictionary<string, object>?> ParseJwt(string accessToken)
    {
        var response = await SendUserInfoRequest(accessToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshResult = await TryRefresh();

            if (refreshResult.IsSuccess && string.IsNullOrEmpty(refreshResult.Value) ==false)
            {
                response = await SendUserInfoRequest(refreshResult.Value);
            }
        }

        if (response.IsSuccessStatusCode == false)
        {
            return null;
        }

        var userInfo = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return userInfo ?? throw new InvalidOperationException();
    }

    private async Task<HttpResponseMessage> SendUserInfoRequest(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "connect/userinfo");
        request.Headers.Authorization = new("Bearer", token);
        return await client.SendAsync(request);
    }

    private async Task<Result<string>> TryRefresh()
    {
        var accessToken = await localStorage.GetItemAsync<string>(AuthenticationService.AccessTokenKey);
        var refreshToken = await localStorage.GetItemAsync<string>(AuthenticationService.RefreshTokenKey);

        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Failure<string>("Токены доступа или обновления отсутствуют.");
        }

        using var requestContent = new FormUrlEncodedContent([
            new("grant_type", "refresh_token"),
            new("refresh_token", refreshToken),
        ]);

        client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);

        var response = await client.PostAsync(new Uri("connect/token", UriKind.Relative), requestContent);

        client.DefaultRequestHeaders.Clear();

        if (response.IsSuccessStatusCode == false)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await localStorage.RemoveItemsAsync([AuthenticationService.AccessTokenKey, AuthenticationService.RefreshTokenKey]);
                return Result.Failure<string>("Токен обновления недействителен или истек. Требуется повторная авторизация.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorContent);
            var error = problemDetails?.Title ?? "Не удалось обновить токен доступа.";

            return Result.Failure<string>(error);
        }

        var authData = await response.Content.ReadFromJsonAsync<AuthData>();

        if (authData == null)
        {
            return Result.Failure<string>("Некорректный ответ от сервера при обновлении токена.");
        }

        await localStorage.SetItemAsync(AuthenticationService.AccessTokenKey, authData.AccessToken);
        await localStorage.SetItemAsync(AuthenticationService.RefreshTokenKey, authData.RefreshToken);

        return Result.Success(authData.AccessToken);
    }
}
