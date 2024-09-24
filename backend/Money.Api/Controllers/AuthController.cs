using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Money.Data.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Money.Api.Controllers;

[ApiController]
public class AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : ControllerBase
{
    /// <summary>
    ///     Обменивает учетные данные пользователя на токен доступа.
    /// </summary>
    /// <remarks>
    ///     Этот метод обрабатывает запросы на получение токена доступа, используя учетные данные пользователя.
    /// </remarks>
    /// <returns>Токен доступа в формате JSON.</returns>
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ClaimsPrincipal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Exchange()
    {
        OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest()
                                    ?? throw new InvalidOperationException("Не удалось получить запрос OpenID Connect.");

        if (request.IsPasswordGrantType())
        {
            ApplicationUser? user = await userManager.FindByNameAsync(request.Username ?? string.Empty);

            if (user == null)
            {
                AuthenticationProperties properties = new(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The username/password couple is invalid."
                }!);

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            SignInResult result = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);

            if (!result.Succeeded)
            {
                AuthenticationProperties properties = new(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The username/password couple is invalid."
                }!);

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            ClaimsIdentity identity = new(TokenValidationParameters.DefaultAuthenticationType,
                OpenIddictConstants.Claims.Name,
                OpenIddictConstants.Claims.Role);

            identity.SetClaim(OpenIddictConstants.Claims.Subject, await userManager.GetUserIdAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Email, await userManager.GetEmailAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Name, await userManager.GetUserNameAsync(user))
                .SetClaim(OpenIddictConstants.Claims.PreferredUsername, await userManager.GetUserNameAsync(user))
                .SetClaims(OpenIddictConstants.Claims.Role, [.. await userManager.GetRolesAsync(user)]);

            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);
            identity.SetScopes(OpenIddictConstants.Scopes.OfflineAccess);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsRefreshTokenGrantType())
        {
            AuthenticateResult result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            ApplicationUser? user = await userManager.FindByIdAsync(result.Principal.GetClaim(OpenIddictConstants.Claims.Subject));

            if (user == null)
            {
                AuthenticationProperties properties = new(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
                }!);

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (!await signInManager.CanSignInAsync(user))
            {
                AuthenticationProperties properties = new(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                }!);

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            ClaimsIdentity identity = new(result.Principal.Claims,
                TokenValidationParameters.DefaultAuthenticationType,
                OpenIddictConstants.Claims.Name,
                OpenIddictConstants.Claims.Role);

            identity.SetClaim(OpenIddictConstants.Claims.Subject, await userManager.GetUserIdAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Email, await userManager.GetEmailAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Name, await userManager.GetUserNameAsync(user))
                .SetClaim(OpenIddictConstants.Claims.PreferredUsername, await userManager.GetUserNameAsync(user))
                .SetClaims(OpenIddictConstants.Claims.Role, [.. await userManager.GetRolesAsync(user)]);

            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException("The specified grant type is not implemented.");
    }

    /// <summary>
    ///     Возвращает информацию о пользователе.
    /// </summary>
    /// <remarks>
    ///     Этот метод обрабатывает запросы на получение информации о пользователе.
    /// </remarks>
    /// <returns>Информация о пользователе в формате JSON.</returns>
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Userinfo()
    {
        string userId = User.GetClaim(OpenIddictConstants.Claims.Subject)
                        ?? throw new InvalidOperationException();

        ApplicationUser? user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            Dictionary<string, string> errors = new()
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                    OpenIddictConstants.Errors.InvalidToken,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                    "Указанный токен доступа привязан к учетной записи, которая больше не существует."
            };

            return Challenge(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(errors!));
        }

        Dictionary<string, string> claims = User.Claims.ToDictionary(claim => claim.Type, claim => claim.Value, StringComparer.Ordinal);
        return Ok(claims);
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
