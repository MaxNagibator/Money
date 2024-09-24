using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Money.Web.Components;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Money.Web.Services;

public class AuthenticationService(
    HttpClient client,
    AuthenticationStateProvider authStateProvider,
    ILocalStorageService localStorage)
{
    private const string AccessTokenKey = "authToken";
    private const string RefreshTokenKey = "refreshToken";

    public async Task Register(UserDto user)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("Account/register", new { user.Email, user.Password });
    }

    public async Task Login(UserDto user)
    {
        AuthData result = await LoginAsync(user.Email, user.Password);

        await localStorage.SetItemAsync(AccessTokenKey, result.AccessToken);
        await localStorage.SetItemAsync(RefreshTokenKey, result.RefreshToken);

        await ((AuthStateProvider)authStateProvider).NotifyUserAuthentication(result.AccessToken);
    }

    public async Task<AuthData> LoginAsync(string username, string password)
    {
        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        ]);

        HttpResponseMessage response = await client.PostAsync("connect/token", requestContent);
        return await response.Content.ReadFromJsonAsync<AuthData>() ?? throw new InvalidOperationException();
    }

    public async Task Logout()
    {
        await localStorage.RemoveItemsAsync([AccessTokenKey, RefreshTokenKey]);
        ((AuthStateProvider)authStateProvider).NotifyUserLogout();
    }

    public async Task<string> RefreshToken()
    {
        string? token = await localStorage.GetItemAsync<string>(AccessTokenKey);
        string? refreshToken = await localStorage.GetItemAsync<string>(RefreshTokenKey);

        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken)
        ]);

        requestContent.Headers.Add("Authorization", $"Bearer {token}");
        HttpResponseMessage response = await client.PostAsync("connect/token", requestContent);
        AuthData? result = await response.Content.ReadFromJsonAsync<AuthData>();

        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException("Something went wrong during the refresh token action");
        }

        await localStorage.SetItemAsync(AccessTokenKey, result.AccessToken);
        await localStorage.SetItemAsync(RefreshTokenKey, result.RefreshToken);
        return result.AccessToken;
    }
}

public record AuthData(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("token_type")]
    string TokenType,
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn,
    [property: JsonPropertyName("scope")]
    string Scope,
    [property: JsonPropertyName("refresh_token")]
    string RefreshToken);
