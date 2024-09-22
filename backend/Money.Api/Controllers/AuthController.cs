using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Money.Business.Services;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Money.Api.Controllers;

public class AuthController(AuthService authorizationService) : Controller
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
}
