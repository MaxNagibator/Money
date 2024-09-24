using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Money.Web;

public class AuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient, JwtParser jwtParser)
    : AuthenticationStateProvider
{
    private readonly AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = await localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return _anonymous;
        }

        ClaimsPrincipal? authenticatedUser = await jwtParser.ValidateJwt(token);
        if(authenticatedUser == null)
        {
            Task<AuthenticationState> authState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(authState);
            return _anonymous;
        }
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        return new AuthenticationState(authenticatedUser);
    }

    public async Task NotifyUserAuthentication(string token)
    {
        ClaimsPrincipal authenticatedUser = await jwtParser.ValidateJwt(token);
        Task<AuthenticationState> authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        Task<AuthenticationState> authState = Task.FromResult(_anonymous);
        NotifyAuthenticationStateChanged(authState);
    }
}
