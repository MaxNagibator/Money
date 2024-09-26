using Blazored.LocalStorage;
using System.Security.Claims;

namespace Money.Web.Services;

public class AuthStateProvider(ILocalStorageService localStorage, JwtParser jwtParser)
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

        if (authenticatedUser == null)
        {
            Task<AuthenticationState> authState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(authState);
            return _anonymous;
        }

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
