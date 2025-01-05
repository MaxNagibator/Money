using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Money.Data.Entities;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace Money.Business.Services;

public class AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
{
    public async Task<ClaimsIdentity> HandlePasswordGrantAsync(OpenIddictRequest request)
    {
        if (request.Username == null)
        {
            throw new PermissionException("Имя пользователя отсутствует в запросе.");
        }

        if (request.Password == null)
        {
            throw new PermissionException("Пароль отсутствует в запросе.");
        }

        var user = await userManager.FindByNameAsync(request.Username);

        if (user == null)
        {
            throw new PermissionException("Неверное имя пользователя или пароль.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (result.Succeeded == false)
        {
            throw new PermissionException("Неверное имя пользователя или пароль.");
        }

        return await CreateClaimsIdentityAsync(user, request.GetScopes().Add(OpenIddictConstants.Scopes.OfflineAccess));
    }

    public async Task<ClaimsIdentity> HandleRefreshTokenGrantAsync(AuthenticateResult result)
    {
        var userId = result.Principal?.GetClaim(OpenIddictConstants.Claims.Subject);

        if (userId == null)
        {
            throw new PermissionException("Не удалось получить идентификатор пользователя.");
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            throw new PermissionException("Токен обновления больше не действителен. Пожалуйста, выполните вход заново.");
        }

        if (await signInManager.CanSignInAsync(user) == false)
        {
            throw new PermissionException("Вам больше не разрешено входить в систему.");
        }

        return await CreateClaimsIdentityAsync(user, null);
    }

    public async Task<Dictionary<string, string>> GetUserInfoAsync(ClaimsPrincipal principal)
    {
        var userId = principal.GetClaim(OpenIddictConstants.Claims.Subject)
                     ?? throw new InvalidOperationException("Не удалось получить идентификатор пользователя.");

        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            throw new PermissionException("Извините, но учетная запись, связанная с этим токеном доступа, больше не существует.");
        }

        return principal.Claims.ToDictionary(claim => claim.Type, claim => claim.Value, StringComparer.Ordinal);
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.PreferredUsername:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject != null && claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Profile))
                {
                    yield return OpenIddictConstants.Destinations.IdentityToken;
                }

                yield break;

            case OpenIddictConstants.Claims.Email:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject != null && claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Email))
                {
                    yield return OpenIddictConstants.Destinations.IdentityToken;
                }

                yield break;

            case OpenIddictConstants.Claims.Role:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject != null && claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Roles))
                {
                    yield return OpenIddictConstants.Destinations.IdentityToken;
                }

                yield break;

            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield break;
        }
    }

    private async Task<ClaimsIdentity> CreateClaimsIdentityAsync(ApplicationUser user, IEnumerable<string>? scopes)
    {
        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role);

        var userIdTask = userManager.GetUserIdAsync(user);
        var emailTask = userManager.GetEmailAsync(user);
        var userNameTask = userManager.GetUserNameAsync(user);
        var rolesTask = userManager.GetRolesAsync(user);

        await Task.WhenAll(userIdTask, emailTask, userNameTask, rolesTask);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, userIdTask.Result)
            .SetClaim(OpenIddictConstants.Claims.Email, emailTask.Result)
            .SetClaim(OpenIddictConstants.Claims.Name, userNameTask.Result)
            .SetClaim(OpenIddictConstants.Claims.PreferredUsername, userNameTask.Result)
            .SetClaims(OpenIddictConstants.Claims.Role, [.. rolesTask.Result]);

        if (scopes != null)
        {
            identity.SetScopes(scopes);
        }

        identity.SetDestinations(GetDestinations);
        return identity;
    }
}
