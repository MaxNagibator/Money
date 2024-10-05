using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Payments;
using Money.Business.Models;
using Money.Business.Services;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class PaymentsController(PaymentService paymentService) : ControllerBase
{
    /// <summary>
    ///     Получить список платежей.
    /// </summary>
    /// <param name="filter">Фильтр.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив платежей.</returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(PaymentDto[]), StatusCodes.Status200OK)]
    public async Task<PaymentDto[]> Get([FromQuery] PaymentFilterDto filter, CancellationToken cancellationToken)
    {
        var businessFilter = filter.ToBusinessModel();
        ICollection<Payment> payments = await paymentService.GetAsync(businessFilter, cancellationToken);
        return payments.Select(PaymentDto.FromBusinessModel).ToArray();
    }
}
