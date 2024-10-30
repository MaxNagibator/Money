using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Payments;
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
        PaymentFilter businessFilter = filter.ToBusinessModel();
        ICollection<Operation> payments = await paymentService.GetAsync(businessFilter, cancellationToken);
        return payments.Select(PaymentDto.FromBusinessModel).ToArray();
    }

    /// <summary>
    ///     Получить платеж по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Информация о платеже.</returns>
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<PaymentDto> GetById(int id, CancellationToken cancellationToken)
    {
        Operation category = await paymentService.GetByIdAsync(id, cancellationToken);
        return PaymentDto.FromBusinessModel(category);
    }

    /// <summary>
    ///     Создать новый платеж.
    /// </summary>
    /// <param name="request">Данные для создания нового платежа.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Идентификатор созданного платежа.</returns>
    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<int> CreateAsync([FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        Operation business = request.ToBusinessModel();
        int id = await paymentService.CreateAsync(business, cancellationToken);
        return id;
    }

    /// <summary>
    ///     Обновить существующий платеж.
    /// </summary>
    /// <param name="id">Идентификатор платежа.</param>
    /// <param name="request">Данные для обновления платежа.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPut]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Update(int id, [FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        Operation business = request.ToBusinessModel();
        business.Id = id;
        await paymentService.UpdateAsync(business, cancellationToken);
    }

    /// <summary>
    ///     Удалить категорию платежа по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Delete(int id, CancellationToken cancellationToken)
    {
        await paymentService.DeleteAsync(id, cancellationToken);
    }

    /// <summary>
    ///     Восстановить категорию платежа по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost]
    [Route("{id:int}/Restore")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Restore(int id, CancellationToken cancellationToken)
    {
        await paymentService.RestoreAsync(id, cancellationToken);
    }

    /// <summary>
    ///     Получить список мест на основе указанного сдвига, количества и необязательной фильтрации по имени.
    /// </summary>
    /// <param name="offset">Сдвиг.</param>
    /// <param name="count">Количество.</param>
    /// <param name="name">Необязательный фильтр по имени.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Список мест.</returns>
    [HttpGet]
    [Route("GetPlaces/{offset:int}/{count:int}")]
    [Route("GetPlaces/{offset:int}/{count:int}/{name}")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<string[]> GetPlaces(int offset, int count, string? name = null, CancellationToken cancellationToken = default)
    {
        ICollection<Place> places = await paymentService.GetPlaces(offset, count, name, cancellationToken);
        return places.Select(x => x.Name).ToArray();
    }
}
