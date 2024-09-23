using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Money.Business.Services;
using Money.Data.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
public class AuthController(AuthService authorizationService, UserManager<ApplicationUser> userManager) : ControllerBase
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

        ClaimsPrincipal claims = await authorizationService.ExchangeAsync(request);
        return SignIn(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
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
}
