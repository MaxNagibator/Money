using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Constracts.Operations;
using Money.Api.Extensions;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class OperationsController(OperationService operationService) : ControllerBase, IOperationsResource
{
    /// <summary>
    ///     Получить список операций.
    /// </summary>
    /// <param name="filter">Фильтр.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив операций.</returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(OperationDTO[]), StatusCodes.Status200OK)]
    public async Task<IEnumerable<OperationDTO>> GetListAsync([FromQuery] OperationDTOFilter filter, CancellationToken cancellationToken)
    {
        OperationFilter businessFilter = filter.ToBusinessFilter();
        ICollection<Operation> operations = await operationService.GetAsync(businessFilter, cancellationToken);
        return operations.Select(OperationExtensions.ToOperationDTO);
    }

    /// <summary>
    ///     Получить операцию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Информация об операции.</returns>
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(typeof(OperationDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<OperationDTO> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Operation category = await operationService.GetByIdAsync(id, cancellationToken);
        return category.ToOperationDTO();
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
    public async Task<int> CreateAsync([FromBody] OperationDTODetails request, CancellationToken cancellationToken)
    {
        Operation business = request.ToBusinessOperation();
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
    public async Task UpdateAsync(int id, [FromBody] OperationDTODetails request, CancellationToken cancellationToken)
    {
        Operation business = request.ToBusinessOperation(id);
        await operationService.UpdateAsync(business, cancellationToken);
    }

    /// <summary>
    ///     Удалить категорию операции по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
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
    public async Task RestoreAsync(int id, CancellationToken cancellationToken)
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
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<string[]> GetPlacesAsync(int offset, int count, string? name = null, CancellationToken cancellationToken = default)
    {
        ICollection<Place> places = await operationService.GetPlaces(offset, count, name, cancellationToken);
        return places.Select(x => x.Name).ToArray();
    }

    Task<string[]> IOperationsResource.GetPlacesAsync(int offset, int count, CancellationToken cancellationToken)
    {
        return GetPlacesAsync(offset, count, null, cancellationToken);
    }
}
