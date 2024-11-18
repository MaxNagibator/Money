using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Operations;
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
    [HttpGet("")]
    [ProducesResponseType(typeof(IEnumerable<OperationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([FromQuery] OperationFilterDto filter, CancellationToken cancellationToken)
    {
        OperationFilter businessFilter = filter.ToBusinessModel();
        IEnumerable<Operation> operations = await operationService.GetAsync(businessFilter, cancellationToken);
        return Ok(operations.Select(OperationDto.FromBusinessModel));
    }

    /// <summary>
    ///     Получить операцию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Информация об операции.</returns>
    [HttpGet("{id:int}", Name = nameof(OperationsController) + nameof(GetById))]
    [ProducesResponseType(typeof(OperationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        Operation category = await operationService.GetByIdAsync(id, cancellationToken);
        return Ok(OperationDto.FromBusinessModel(category));
    }

    /// <summary>
    ///     Создать новую операцию.
    /// </summary>
    /// <param name="request">Данные для создания новой операции.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Идентификатор созданной операции.</returns>
    [HttpPost("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync([FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        Operation business = request.ToBusinessModel();
        int result = await operationService.CreateAsync(business, cancellationToken);
        return CreatedAtRoute(nameof(OperationsController) + nameof(GetById), new { id = result }, result);
    }

    /// <summary>
    ///     Обновить существующую операцию.
    /// </summary>
    /// <param name="id">Идентификатор операции.</param>
    /// <param name="request">Данные для обновления операции.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        Operation business = request.ToBusinessModel();
        business.Id = id;
        await operationService.UpdateAsync(business, cancellationToken);
        return Ok();
    }

    /// <summary>
    ///     Обновить категорию у операций.
    /// </summary>
    /// <param name="request">Данные для обновления операций.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost("UpdateBatch")]
    [ProducesResponseType(typeof(IEnumerable<OperationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await operationService.DeleteAsync(id, cancellationToken);
        return Ok();
    }

    /// <summary>
    ///     Восстановить категорию операции по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost("{id:int}/Restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        await operationService.RestoreAsync(id, cancellationToken);
        return Ok();
    }

    /// <summary>
    ///     Получить список мест на основе указанного сдвига, количества и необязательной фильтрации по имени.
    /// </summary>
    /// <param name="offset">Сдвиг.</param>
    /// <param name="count">Количество.</param>
    /// <param name="name">Необязательный фильтр по имени.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Список мест.</returns>
    [HttpGet("GetPlaces/{offset:int}/{count:int}")]
    [HttpGet("GetPlaces/{offset:int}/{count:int}/{name}")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlaces(int offset, int count, string? name = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Place> places = await operationService.GetPlacesAsync(offset, count, name, cancellationToken);
        return Ok(places.Select(x => x.Name));
    }
}
