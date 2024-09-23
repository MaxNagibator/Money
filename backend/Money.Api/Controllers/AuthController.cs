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

public class AuthController(AuthService authorizationService, UserManager<ApplicationUser> userManager) : Controller
{
    /// <summary>
    ///     Обменять учетные данные на токен доступа.
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

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    public async Task<IActionResult> Userinfo()
    {
        ApplicationUser? user = await userManager.FindByIdAsync(User.GetClaim(OpenIddictConstants.Claims.Subject));

        if (user == null)
        {
            return Challenge(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is bound to an account that no longer exists."
                }));
        }

        Dictionary<string, object> claims = new(StringComparer.Ordinal);

        foreach (Claim claim in User.Claims)
        {
            claims[claim.Type] = claim.Value;
        }

        return Ok(claims);
    }
}
