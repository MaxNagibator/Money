using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Operations;
using Money.Api.Extensions;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class OperationsController(OperationService operationService) : ControllerBase
{
    /// <summary>
    ///     Получить список операций.
    /// </summary>
    /// <param name="filter">Фильтр.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив операций.</returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(OperationDto[]), StatusCodes.Status200OK)]
    public async Task<OperationDto[]> Get([FromQuery] OperationFilterDto filter, CancellationToken cancellationToken)
    {
        OperationFilter businessFilter = filter.ToBusinessModel();
        IEnumerable<Operation> operations = await operationService.GetAsync(businessFilter, cancellationToken);
        return operations.Select(OperationDto.FromBusinessModel).ToArray();
    }

    /// <summary>
    ///     Получить операцию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Информация об операции.</returns>
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(typeof(OperationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<OperationDto> GetById(int id, CancellationToken cancellationToken)
    {
        Operation category = await operationService.GetByIdAsync(id, cancellationToken);
        return OperationDto.FromBusinessModel(category);
    }

    /// <summary>
    ///     Создать новую операцию.
    /// </summary>
    /// <param name="request">Данные для создания новой операции.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Идентификатор созданной операции.</returns>
    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<int> CreateAsync([FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        Operation business = request.ToBusinessModel();
        int id = await operationService.CreateAsync(business, cancellationToken);
        return id;
    }

    /// <summary>
    ///     Обновить существующую операцию.
    /// </summary>
    /// <param name="id">Идентификатор операции.</param>
    /// <param name="request">Данные для обновления операции.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPut]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Update(int id, [FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        Operation business = request.ToBusinessModel();
        business.Id = id;
        await operationService.UpdateAsync(business, cancellationToken);
    }

    /// <summary>
    ///     Обновить категорию у операций.
    /// </summary>
    /// <param name="request">Данные для обновления операций.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost]
    [Route("UpdateBatch")]
    [ProducesResponseType(typeof(IEnumerable<OperationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBatch(UpdateOperationsBatchRequest request, CancellationToken cancellationToken)
    {
        IEnumerable<Operation> updatedOperations = await operationService.UpdateBatchAsync(request.OperationIds, request.CategoryId, cancellationToken);
        return Ok(updatedOperations.Select(OperationDto.FromBusinessModel));
    }

    /// <summary>
    ///     Удалить категорию операции по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Delete(int id, CancellationToken cancellationToken)
    {
        await operationService.DeleteAsync(id, cancellationToken);
    }

    /// <summary>
    ///     Восстановить категорию операции по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost]
    [Route("{id:int}/Restore")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Restore(int id, CancellationToken cancellationToken)
    {
        await operationService.RestoreAsync(id, cancellationToken);
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
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlaces(int offset, int count, string? name = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Place> places = await operationService.GetPlacesAsync(offset, count, name, cancellationToken);
        return Ok(places.Select(x => x.Name));
    }
}
