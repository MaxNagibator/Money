using Blazored.LocalStorage;
using CSharpFunctionalExtensions;
using Money.ApiClient;
using System.Net.Http.Json;
using System.Text.Json;
using AuthData = Money.Web.Models.AuthData;

namespace Money.Web.Services.Authentication;

public class AuthenticationService(
    HttpClient client,
    AuthenticationStateProvider authStateProvider,
    ILocalStorageService localStorage)
{
    public const string AccessTokenKey = "authToken";
    public const string RefreshTokenKey = "refreshToken";

    public async Task<Result> RegisterAsync(UserDto user)
    {
        var response = await client.PostAsJsonAsync("Account/register", new { user.Email, user.Password });

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
        var requestContent = new FormUrlEncodedContent([new("grant_type", "password"), new("username", user.Email), new("password", user.Password)]);

        var result = await AuthenticateAsync(requestContent);

        return await result.Tap(async () => await ((AuthStateProvider)authStateProvider).NotifyUserAuthentication());
    }

    public async Task<Result> LogoutAsync()
    {
        await localStorage.RemoveItemsAsync([AccessTokenKey, RefreshTokenKey]);
        await ((AuthStateProvider)authStateProvider).NotifyUserAuthentication();
        return Result.Success();
    }

    public async Task<Result<string>> RefreshTokenAsync()
    {
        var token = await localStorage.GetItemAsync<string>(AccessTokenKey);
        var refreshToken = await localStorage.GetItemAsync<string>(RefreshTokenKey);

        if (token == null || refreshToken == null)
        {
            return Result.Failure<string>("Не удалось загрузить токен доступа или токен обновления.");
        }

        var requestContent = new FormUrlEncodedContent([new("grant_type", "refresh_token"), new("refresh_token", refreshToken)]);

        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        var result = await AuthenticateAsync(requestContent);
        return result.Map(data => data.AccessToken);
    }

    private async Task<Result<AuthData>> AuthenticateAsync(FormUrlEncodedContent requestContent)
    {
        var response = await client.PostAsync("connect/token", requestContent);
        client.DefaultRequestHeaders.Clear();

        if (response.IsSuccessStatusCode == false)
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
}
