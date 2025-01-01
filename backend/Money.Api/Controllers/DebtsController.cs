using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Debts;
using Money.Api.Dto.FastOperations;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class DebtsController(DebtService debtService) : ControllerBase
{
    /// <summary>
    ///     Получить список долгов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив операций.</returns>
    [HttpGet("")]
    [ProducesResponseType(typeof(IEnumerable<FastOperationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        IEnumerable<Debt> debts = await debtService.GetAsync(false, cancellationToken);
        return Ok(debts.Select(DebtDto.FromBusinessModel));
    }
}
