using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Accounts;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class AccountController(AccountService accountService) : ControllerBase
{
    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    /// <remarks>
    /// Этот метод позволяет зарегистрировать нового пользователя в системе.
    /// </remarks>
    /// <param name="model">Данные пользователя для регистрации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Статус выполнения операции.</returns>
    [HttpPost("Register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model, CancellationToken cancellationToken)
    {
        await accountService.RegisterAsync(model, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Подтверждение почты.
    /// </summary>
    /// <param name="model">Данные для подтверждения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Статус выполнения операции.</returns>
    [HttpPost("ConfirmEmail")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailDto model, CancellationToken cancellationToken)
    {
        await accountService.ConfirmEmailAsync(model.ConfirmCode, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Повторная отправка кода подтверждения.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Статус выполнения операции.</returns>
    [HttpPost("ResendConfirmCode")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmCodeAsync(CancellationToken cancellationToken)
    {
        await accountService.ResendConfirmCodeAsync(cancellationToken);
        return NoContent();
    }
}
