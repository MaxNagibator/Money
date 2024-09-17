using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Money.Common.Exceptions;
using Money.Data.Entities;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Money.Business.Services;

public class AuthService(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager)
{
    public async Task<ClaimsPrincipal> ExchangeAsync(OpenIddictRequest request)
    {
        if (request.IsPasswordGrantType() == false)
        {
            throw new IncorrectDataException("Указанный тип гранта не реализован");
        }

        ApplicationUser? user = await userManager.FindByNameAsync(request.Username ?? string.Empty);

        if (user == null)
        {
            throw new IncorrectDataException("Пара логин/пароль недействительна");
        }

        SignInResult? signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (signInResult.Succeeded == false)
        {
            throw new IncorrectDataException("Пара логин/пароль недействительна");
        }

        Task<string> userIdTask = userManager.GetUserIdAsync(user);
        Task<string?> emailTask = userManager.GetEmailAsync(user);
        Task<string?> userNameTask = userManager.GetUserNameAsync(user);
        Task<IList<string>> rolesTask = userManager.GetRolesAsync(user);

        await Task.WhenAll(userIdTask, emailTask, userNameTask, rolesTask);

        ClaimsIdentity identity = new(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

        identity.SetClaim(Claims.Subject, userIdTask.Result)
            .SetClaim(Claims.Email, emailTask.Result)
            .SetClaim(Claims.Name, userNameTask.Result)
            .SetClaim(Claims.PreferredUsername, userNameTask.Result)
            .SetClaims(Claims.Role, [.. rolesTask.Result]);

        IEnumerable<string> scopes = new[]
        {
            Scopes.OpenId,
            Scopes.Email,
            Scopes.Profile,
            Scopes.Roles
        }.Intersect(request.GetScopes());

        identity.SetScopes(scopes);
        identity.SetDestinations(GetDestinations);

        ClaimsPrincipal claimsPrincipal = new(identity);
        return claimsPrincipal;
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Name or Claims.PreferredUsername:
                yield return Destinations.AccessToken;

                if (claim.Subject != null && claim.Subject.HasScope(Permissions.Scopes.Profile))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (claim.Subject != null && claim.Subject.HasScope(Permissions.Scopes.Email))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (claim.Subject != null && claim.Subject.HasScope(Permissions.Scopes.Roles))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}
