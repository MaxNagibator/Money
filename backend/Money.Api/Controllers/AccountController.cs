using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Constracts.Accounts;

namespace Money.Api.Controllers;

[Authorize]
[Route("[controller]")]
public class AccountController(AccountService accountService) : ControllerBase, IAccountsResource
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
    public Task RegisterAsync([FromBody] AccountRegisterInfo model, CancellationToken cancellationToken = default)
    {
        RegisterModel bisnessModel = new()
        {
            Email = model.Email,
            Password = model.Password,
        };

        return accountService.RegisterAsync(bisnessModel, cancellationToken);
    }
}
