using Blazored.LocalStorage;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Money.ApiClient;
using System.Net.Http.Json;
using System.Text.Json;
using AuthData = Money.Web.Models.AuthData;

namespace Money.Web.Services.Authentication;

public class AuthenticationService(HttpClient client, ILocalStorageService localStorage)
{
    public const string AccessTokenKey = "authToken";
    public const string RefreshTokenKey = "refreshToken";

    public async Task<Result> RegisterAsync(RegisterUserDto user)
    {
        var response = await client.PostAsJsonAsync("Accounts/register", new { user.UserName, user.Email, user.Password });

        if (response.IsSuccessStatusCode)
        {
            return Result.Success();
        }

        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync());
        var error = problemDetails?.Title ?? "Ошибка регистрации аккаунта на сервере.";
        return Result.Failure(error);
    }

    public async Task<Result> LoginAsync(UserDto user)
    {
        using var requestContent = new FormUrlEncodedContent([new("grant_type", "password"), new("username", user.Login), new("password", user.Password)]);
        var result = await AuthenticateAsync(requestContent, null);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }

    public async Task<Result> LogoutAsync()
    {
        var refreshToken = await localStorage.GetItemAsync<string>(RefreshTokenKey);

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await TryRevokeAsync(refreshToken);
        }

        await localStorage.RemoveItemsAsync([AccessTokenKey, RefreshTokenKey]);
        return Result.Success();
    }

    public string GetExternalAuthUrl(string provider, string returnUrl)
    {
        var encoded = Uri.EscapeDataString(returnUrl);
        return client.BaseAddress + $"external/login/{provider}?returnUrl={encoded}";
    }

    public async Task<Result> ExchangeExternalAsync()
    {
        using var requestContent = new FormUrlEncodedContent([new("grant_type", "external")]);
        var result = await AuthenticateAsync(requestContent, null);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }

    public async Task<Result<string>> RefreshTokenAsync()
    {
        var token = await localStorage.GetItemAsync<string>(AccessTokenKey);
        var refreshToken = await localStorage.GetItemAsync<string>(RefreshTokenKey);

        if (token == null || refreshToken == null)
        {
            return Result.Failure<string>("Не удалось загрузить токен доступа или токен обновления.");
        }

        using var requestContent = new FormUrlEncodedContent([new("grant_type", "refresh_token"), new("refresh_token", refreshToken)]);
        var result = await AuthenticateAsync(requestContent, token);
        return result.Map(data => data.AccessToken);
    }

    private async Task<Result<AuthData>> AuthenticateAsync(FormUrlEncodedContent requestContent, string? authorizationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("connect/token", UriKind.Relative));

        request.Content = requestContent;
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        if (authorizationToken != null)
        {
            request.Headers.Authorization = new("Bearer", authorizationToken);
        }

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync());
            var error = problemDetails?.Title ?? "Не удалось получить данные авторизации.";
            return Result.Failure<AuthData>(error);
        }

        var result = await response.Content.ReadFromJsonAsync<AuthData>();

        if (result == null)
        {
            return Result.Failure<AuthData>("Некорректный ответ от сервера.");
        }

        await localStorage.SetItemAsync(AccessTokenKey, result.AccessToken);
        await localStorage.SetItemAsync(RefreshTokenKey, result.RefreshToken);
        return result;
    }

    private async Task TryRevokeAsync(string refreshToken)
    {
        try
        {
            using var requestContent = new FormUrlEncodedContent([
                new("token", refreshToken),
                new("token_type_hint", "refresh_token"),
            ]);

            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("connect/revoke", UriKind.Relative))
            {
                Content = requestContent,
            };

            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            await client.SendAsync(request);
        }
        catch (HttpRequestException)
        {
        }
    }
}
