using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Money.Data.Entities;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace Money.Business.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context)
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
            user = await userManager.FindByEmailAsync(request.Username);

            if (user == null || user.EmailConfirmed == false)
            {
                throw new PermissionException("Неверное имя пользователя или пароль.");
            }
        }

        var domainUser = await context.DomainUsers.SingleAsync(x => x.AuthUserId == user.Id);

        if (domainUser.TransporterPassword != null)
        {
            if (!LegacyAuth.Validate(request.Username, request.Password, domainUser.TransporterPassword))
            {
                throw new PermissionException("Неверное имя пользователя или пароль.");
            }
        }
        else
        {
            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);

            if (result.Succeeded == false)
            {
                throw new PermissionException("Неверное имя пользователя или пароль.");
            }
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

        var originalScopes = result.Principal?.GetScopes() ?? [];

        var scopes = originalScopes.Contains(OpenIddictConstants.Scopes.OfflineAccess)
            ? originalScopes
            : originalScopes.Add(OpenIddictConstants.Scopes.OfflineAccess);

        return await CreateClaimsIdentityAsync(user, scopes);
    }

    public async Task<ClaimsIdentity> HandleExternalGrantAsync(AuthenticateResult result)
    {
        var nameId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? result.Principal?.FindFirstValue(OpenIddictConstants.Claims.Subject);

        if (nameId == null)
        {
            throw new PermissionException("Не удалось получить идентификатор пользователя.");
        }

        var user = await userManager.GetUserAsync(result.Principal!)
                   ?? await userManager.FindByNameAsync(result.Principal!.Identity?.Name ?? string.Empty)
                   ?? await userManager.FindByEmailAsync(result.Principal!.FindFirstValue(ClaimTypes.Email) ?? string.Empty);

        if (user == null)
        {
            throw new PermissionException("Извините, но учетная запись пользователя не найдена.");
        }

        if (await signInManager.CanSignInAsync(user) == false)
        {
            throw new PermissionException("Вам больше не разрешено входить в систему.");
        }

        var scopes = new[]
        {
            OpenIddictConstants.Scopes.OfflineAccess,
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles,
        };

        return await CreateClaimsIdentityAsync(user, scopes);
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

        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var group in principal.Claims.GroupBy(x => x.Type, StringComparer.Ordinal))
        {
            var uniqueValues = group.Select(x => x.Value)
                .Where(x => x != null)
                .Distinct(StringComparer.Ordinal);

            var joined = string.Join(' ', uniqueValues);
            result[group.Key] = joined;
        }

        return result;
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

            case OpenIddictConstants.Claims.Email or OpenIddictConstants.Claims.EmailVerified:
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
        var emailConfirmedTask = userManager.IsEmailConfirmedAsync(user);

        await Task.WhenAll(userIdTask, emailTask, userNameTask, rolesTask);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, await userIdTask)
            .SetClaim(OpenIddictConstants.Claims.Email, await emailTask)
            .SetClaim(OpenIddictConstants.Claims.Name, await userNameTask)
            .SetClaim(OpenIddictConstants.Claims.EmailVerified, await emailConfirmedTask)
            .SetClaim(OpenIddictConstants.Claims.PreferredUsername, await userNameTask)
            .SetClaims(OpenIddictConstants.Claims.Role, [.. await rolesTask]);

        if (scopes != null)
        {
            identity.SetScopes(scopes);
        }

        identity.SetDestinations(GetDestinations);
        return identity;
    }
}
