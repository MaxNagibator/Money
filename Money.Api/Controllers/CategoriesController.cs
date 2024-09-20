using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto;
using Money.Business.Services;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class CategoriesController(ILogger<CategoriesController> logger, PaymentCategoryService paymentCategoryService) : ControllerBase
{
    /// <summary>
    ///     Получить список категорий платежей.
    /// </summary>
    /// <param name="type">Тип категории (необязательный).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список категорий платежей.</returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(GetCategoriesResponse), StatusCodes.Status200OK)]
    public async Task<GetCategoriesResponse> Get([FromQuery] int? type, CancellationToken cancellationToken)
    {
        ICollection<Business.Models.PaymentCategory> categories = await paymentCategoryService.GetAsync(type, cancellationToken);
        return new GetCategoriesResponse(categories);
    }

    /// <summary>
    ///     Получить категорию платежа по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <returns>Информация о категории.</returns>
    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public string GetById(Guid id)
    {
        return "byid" + id;
    }

    /// <summary>
    ///     Создать новую категорию платежа.
    /// </summary>
    /// <param name="request">Данные для создания категории.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Идентификатор созданной категории.</returns>
    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<int> CreateAsync([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        Business.Models.PaymentCategory business = request.GetBusinessModel();
        int id = await paymentCategoryService.CreateAsync(business, cancellationToken);
        return id;
    }

    /// <summary>
    ///     Обновить существующую категорию платежа.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <returns>Сообщение об обновлении.</returns>
    [HttpPut]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public string Update(Guid id)
    {
        return "update";
    }

    /// <summary>
    ///     Удалить категорию платежа по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <returns>Сообщение об удалении.</returns>
    [HttpDelete]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public string Delete(Guid id)
    {
        return "delete";
    }
}
