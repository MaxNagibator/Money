using System.Collections.Immutable;
using System.Security.Claims;
using Money.Common.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Money.BusinessLogic.Interfaces;
using OpenIddict.Abstractions;
using Money.Data.Entities;

namespace Money.BusinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<ClaimsPrincipal> ExchangeAsync(OpenIddictRequest request)
        {
            if (request.IsPasswordGrantType() == false)
            {
                throw new IncorrectDataException("Указанный тип гранта не реализован");
            }

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                throw new IncorrectDataException("Пара логин/пароль недействительна");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
            if (signInResult.Succeeded == false)
            {
                throw new IncorrectDataException("Пара логин/пароль недействительна");
            }

            var userIdTask = _userManager.GetUserIdAsync(user);
            var emailTask = _userManager.GetEmailAsync(user);
            var userNameTask = _userManager.GetUserNameAsync(user);
            var rolesTask = _userManager.GetRolesAsync(user);

            await Task.WhenAll(userIdTask, emailTask, userNameTask, rolesTask);

            var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType,
                OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role);

            identity.SetClaim(OpenIddictConstants.Claims.Subject, userIdTask.Result)
                .SetClaim(OpenIddictConstants.Claims.Email, emailTask.Result)
                .SetClaim(OpenIddictConstants.Claims.Name, userNameTask.Result)
                .SetClaim(OpenIddictConstants.Claims.PreferredUsername, userNameTask.Result)
                .SetClaims(OpenIddictConstants.Claims.Role, rolesTask.Result.ToImmutableArray());

            var scopes = new[]
            {
                OpenIddictConstants.Scopes.OpenId,
                OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Scopes.Roles
            }.Intersect(request.GetScopes());

            identity.SetScopes(scopes);
            identity.SetDestinations(GetDestinations);

            var claimsPrincipal = new ClaimsPrincipal(identity);
            return claimsPrincipal;
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
}
