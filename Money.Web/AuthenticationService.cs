using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using OpenIddict.Client;

namespace Money.Web;

public class AuthenticationService(
    HttpClient client,
    AuthenticationStateProvider authStateProvider,
    ILocalStorageService localStorage,
    OpenIddictClientService openIddictClientService)
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public async Task Register(UserDto user)
    {
        var response = await client.PostAsJsonAsync("https://localhost:7124/Account/register", new { user.Email, user.Password });
    }

    public async Task Login(UserDto user)
    {
       // var result = await openIddictClientService.AuthenticateWithPasswordAsync(new()
       // {
       //     Username = user.Email,
       //     Password = user.Password
       // });

       var result = await LoginAsync(user.Email, user.Password);

        await localStorage.SetItemAsync("authToken", result.AccessToken);
        ((AuthStateProvider)authStateProvider).NotifyUserAuthentication(user.Email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
    }

    public async Task<AuthData> LoginAsync(string username, string password)
    {
        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        ]);

        HttpResponseMessage response = await client.PostAsync("https://localhost:7124/connect/token", requestContent);
        return await response.Content.ReadFromJsonAsync<AuthData>() ?? throw new InvalidOperationException();
    }

    public async Task Logout()
    {
        await localStorage.RemoveItemAsync("authToken");
        ((AuthStateProvider)authStateProvider).NotifyUserLogout();
        client.DefaultRequestHeaders.Authorization = null;
    }
}

public record AuthData(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("token_type")]
    string TokenType,
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn);
