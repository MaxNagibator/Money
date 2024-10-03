using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Payments;
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
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив платежей.</returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(PaymentDto[]), StatusCodes.Status200OK)]
    public async Task<PaymentDto[]> Get(
         [FromQuery] DateTime? dateFrom,
         [FromQuery] DateTime? dateTo,
         [FromQuery] List<int>? categoryIds,
         [FromQuery] string? comment,
         [FromQuery] string? place,
         CancellationToken cancellationToken)
    {
        ICollection<Business.Models.Payment> categories = await paymentService.GetAsync(dateFrom, dateTo, categoryIds, comment, place, cancellationToken);
        return categories.Select(PaymentDto.FromBusinessModel).ToArray();
    }
}