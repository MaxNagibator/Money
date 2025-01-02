using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

namespace Money.Api.Controllers;

[ApiController]
public class AuthController(AuthService authService) : ControllerBase
{
    private const string AuthenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;

    /// <summary>
    /// Обменивает учетные данные пользователя на токен доступа.
    /// </summary>
    /// <remarks>
    /// Этот метод обрабатывает запросы на получение токена доступа, используя учетные данные пользователя.
    /// </remarks>
    /// <returns>Токен доступа в формате JSON.</returns>
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ClaimsPrincipal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("Не удалось получить запрос OpenID Connect.");

        ClaimsIdentity identity;

        if (request.IsPasswordGrantType())
        {
            identity = await authService.HandlePasswordGrantAsync(request);
        }
        else if (request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(AuthenticationScheme);
            identity = await authService.HandleRefreshTokenGrantAsync(result);
        }
        else
        {
            throw new NotImplementedException("Указанный тип предоставления не реализован.");
        }

        return SignIn(new(identity), AuthenticationScheme);
    }

    /// <summary>
    /// Возвращает информацию о пользователе.
    /// </summary>
    /// <remarks>
    /// Этот метод обрабатывает запросы на получение информации о пользователе.
    /// </remarks>
    /// <returns>Информация о пользователе в формате JSON.</returns>
    [Authorize(AuthenticationSchemes = AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Userinfo()
    {
        return Ok(await authService.GetUserInfoAsync(User));
    }
}
