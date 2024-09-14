using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Money.Api.Common.Results;
using Money.Api.Data;
using OpenIddict.Abstractions;

namespace Money.Api.Services;

public class AuthorizationService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
{
    public async Task<Result<ClaimsPrincipal>> ExchangeAsync(OpenIddictRequest request)
    {
        if (request.IsPasswordGrantType() == false)
        {
            return Result.Failure("Указанный тип гранта не реализован");
        }

        ApplicationUser? user = await userManager.FindByNameAsync(request.Username!);

        if (user == null)
        {
            return Result.Failure("Пара логин/пароль недействительна");
        }

        SignInResult result = await signInManager.CheckPasswordSignInAsync(user, request.Password!, true);

        if (result.Succeeded == false)
        {
            return Result.Failure("Пара логин/пароль недействительна");
        }

        ClaimsIdentity identity = new(TokenValidationParameters.DefaultAuthenticationType,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, await userManager.GetUserIdAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Email, await userManager.GetEmailAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Name, await userManager.GetUserNameAsync(user))
            .SetClaim(OpenIddictConstants.Claims.PreferredUsername, await userManager.GetUserNameAsync(user))
            .SetClaims(OpenIddictConstants.Claims.Role, [.. await userManager.GetRolesAsync(user)]);

        identity.SetScopes(new[]
        {
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Roles
        }.Intersect(request.GetScopes()));

        identity.SetDestinations(GetDestinations);

        return Result<ClaimsPrincipal>.Success(new ClaimsPrincipal(identity));
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
}
