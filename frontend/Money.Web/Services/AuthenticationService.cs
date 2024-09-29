using Blazored.LocalStorage;
using System.Net.Http.Json;

namespace Money.Web.Services;

public class AuthenticationService(
    IHttpClientFactory clientFactory,
    AuthenticationStateProvider authStateProvider,
    ILocalStorageService localStorage)
{
    public const string AccessTokenKey = "authToken";
    public const string RefreshTokenKey = "refreshToken";
    private readonly HttpClient _client = clientFactory.CreateClient("api_base");

    public async Task RegisterAsync(UserDto user)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("Account/register", new { user.Email, user.Password });
        response.EnsureSuccessStatusCode();
    }

    public async Task LoginAsync(UserDto user)
    {
        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", user.Email),
            new KeyValuePair<string, string>("password", user.Password),
        ]);

        AuthData result = await AuthenticateAsync(requestContent);
        await ((AuthStateProvider)authStateProvider).NotifyUserAuthentication();
    }

    public async Task LogoutAsync()
    {
        await localStorage.RemoveItemsAsync([AccessTokenKey, RefreshTokenKey]);
        await ((AuthStateProvider)authStateProvider).NotifyUserAuthentication();
    }

    public async Task<string> RefreshTokenAsync()
    {
        string? token = await localStorage.GetItemAsync<string>(AccessTokenKey);
        string? refreshToken = await localStorage.GetItemAsync<string>(RefreshTokenKey);

        if (token == null || refreshToken == null)
        {
            throw new ApplicationException("Не удалось получить токен доступа или токен обновления.");
        }

        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
        ]);

        requestContent.Headers.Add("Authorization", $"Bearer {token}");
        AuthData result = await AuthenticateAsync(requestContent);
        return result.AccessToken;
    }

    private async Task<AuthData> AuthenticateAsync(FormUrlEncodedContent requestContent)
    {
        HttpResponseMessage response = await _client.PostAsync("connect/token", requestContent);
        AuthData? result = await response.Content.ReadFromJsonAsync<AuthData>();

        if (result == null || response.IsSuccessStatusCode == false)
        {
            throw new ApplicationException("Не могу получить данные авторизации.");
        }

        await localStorage.SetItemAsync(AccessTokenKey, result.AccessToken);
        await localStorage.SetItemAsync(RefreshTokenKey, result.RefreshToken);
        return result;
    }
}
