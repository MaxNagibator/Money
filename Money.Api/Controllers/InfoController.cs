using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Money.Data.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class InfoController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    /// <summary>
    ///     Получить информацию о доступе.
    /// </summary>
    /// <returns>Сообщение о том, что доступ разрешен.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok("Я авторизован");
    }

    /// <summary>
    ///     Получить сообщение о пользователе.
    /// </summary>
    /// <returns>Сообщение с именем пользователя, если аутентификация успешна.</returns>
    [HttpGet("message")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMessage()
    {
        ApplicationUser? user = await userManager.FindByIdAsync(User.GetClaim(OpenIddictConstants.Claims.Subject) ?? string.Empty);

        if (user == null)
        {
            return Challenge(authenticationSchemes: OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictValidationAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictValidationAspNetCoreConstants.Properties.ErrorDescription] =
                        "Указанный токен доступа связан с учетной записью, которая больше не существует."
                }));
        }

        return Content($"{user.UserName} успешно аутентифицирован.");
    }
}
