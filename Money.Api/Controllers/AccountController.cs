using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Business.Models;
using Money.Business.Services;

namespace Money.Api.Controllers;

[Authorize]
[Route("[controller]")]
public class AccountController(AccountService accountService) : ControllerBase
{
    /// <summary>
    ///     Регистрация нового пользователя.
    /// </summary>
    /// <remarks>
    ///     Этот метод позволяет зарегистрировать нового пользователя в системе.
    /// </remarks>
    /// <param name="model">Модель регистрации, содержащая данные пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Статус выполнения операции.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }

        await accountService.RegisterAsync(model, cancellationToken);
        return Ok();
    }
}
