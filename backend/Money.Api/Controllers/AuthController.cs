using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Money.Common.Exceptions;
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
            if (request.Username == null)
            {
                throw new PermissionException("Имя пользователя отсутствует в запросе.");
            }

            if (request.Password == null)
            {
                throw new PermissionException("Пароль отсутствует в запросе.");
            }

            ApplicationUser? user = await userManager.FindByNameAsync(request.Username);

            if (user == null)
            {
                throw new PermissionException("Неверное имя пользователя или пароль. Пожалуйста, проверьте введенные данные и попробуйте снова.");
            }

            SignInResult result = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);

            if (result.Succeeded == false)
            {
                throw new PermissionException("Неверное имя пользователя или пароль. Пожалуйста, проверьте введенные данные и попробуйте снова.");
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

            string? userId = result.Principal?.GetClaim(OpenIddictConstants.Claims.Subject);

            if (userId == null)
            {
                throw new PermissionException("Не удалось получить идентификатор пользователя.");
            }

            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new PermissionException("Токен обновления больше не действителен. Пожалуйста, выполните вход заново.");
            }

            if (!await signInManager.CanSignInAsync(user))
            {
                throw new PermissionException("Вам больше не разрешено входить в систему.");
            }

            ClaimsIdentity identity = new(result.Principal?.Claims,
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

        throw new NotImplementedException("Указанный тип предоставления не реализован.");
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
                        ?? throw new InvalidOperationException("Не удалось получить идентификатор пользователя.");

        ApplicationUser? user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            throw new PermissionException("Извините, но учетная запись, связанная с этим токеном доступа, больше не существует.");
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
