using Blazored.LocalStorage;
using System.Security.Claims;

namespace Money.Web.Services.Authentication;

public class AuthStateProvider(ILocalStorageService localStorage, JwtParser jwtParser)
    : AuthenticationStateProvider
{
    private readonly AuthenticationState _anonymous = new(new(new ClaimsIdentity()));
    private readonly TimeSpan _userCacheRefreshInterval = TimeSpan.FromSeconds(60.0);

    private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0L);
    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await GetUser(true);
        return new(user);
    }

    public async Task NotifyUserAuthentication()
    {
        var user = await GetUser();
        var state = new AuthenticationState(user);
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    private async Task<ClaimsPrincipal> GetUser(bool useCache = false)
    {
        var now = DateTimeOffset.Now;

        if (useCache && now < _userLastCheck + _userCacheRefreshInterval)
        {
            return _cachedUser;
        }

        var token = await localStorage.GetItemAsync<string>(AuthenticationService.AccessTokenKey);

        if (string.IsNullOrWhiteSpace(token))
        {
            _cachedUser = _anonymous.User;
            _userLastCheck = now;
            return _cachedUser;
        }

        var user = await jwtParser.ValidateJwt(token);

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
