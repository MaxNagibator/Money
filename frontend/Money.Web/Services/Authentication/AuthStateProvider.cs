using Blazored.LocalStorage;
using System.Security.Claims;

namespace Money.Web.Services.Authentication;

public class AuthStateProvider(ILocalStorageService localStorage, JwtParser jwtParser)
    : AuthenticationStateProvider
{
    private readonly AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));
    private readonly TimeSpan _userCacheRefreshInterval = TimeSpan.FromSeconds(60.0);

    private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0L);
    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return new AuthenticationState(await GetUser(true));
    }

    public async Task NotifyUserAuthentication()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(await GetUser())));
    }

    private async Task<ClaimsPrincipal> GetUser(bool useCache = false)
    {
        DateTimeOffset now = DateTimeOffset.Now;

        if (useCache && now < _userLastCheck + _userCacheRefreshInterval)
        {
            return _cachedUser;
        }

        string? token = await localStorage.GetItemAsync<string>(AuthenticationService.AccessTokenKey);

        if (string.IsNullOrWhiteSpace(token))
        {
            _cachedUser = _anonymous.User;
            _userLastCheck = now;
            return _cachedUser;
        }

        ClaimsPrincipal? user = await jwtParser.ValidateJwt(token);

        if (user == null)
        {
            await localStorage.RemoveItemsAsync([AuthenticationService.AccessTokenKey, AuthenticationService.RefreshTokenKey]);
            user = _anonymous.User;
        }

        _cachedUser = user;
        _userLastCheck = now;
        return _cachedUser;
    }
}
