using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.RegularOperations;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class RegularOperationsController(RegularOperationService regularOperationService) : ControllerBase
{
    /// <summary>
    /// Получить список быстрых операций.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив операций.</returns>
    [HttpGet("")]
    [ProducesResponseType(typeof(IEnumerable<RegularOperationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var operations = await regularOperationService.GetAsync(cancellationToken);
        return Ok(operations.Select(RegularOperationDto.FromBusinessModel));
    }

    /// <summary>
    /// Получить быструю операцию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Информация об операции.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RegularOperationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var operation = await regularOperationService.GetByIdAsync(id, cancellationToken);
        return Ok(RegularOperationDto.FromBusinessModel(operation));
    }

    /// <summary>
    /// Создать новую быструю операцию.
    /// </summary>
    /// <param name="request">Данные для создания новой быстрой операции.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Идентификатор созданной операции.</returns>
    [HttpPost("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        var business = request.ToBusinessModel();
        var result = await regularOperationService.CreateAsync(business, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result }, result);
    }

    /// <summary>
    /// Обновить существующую быструю операцию.
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
        var business = request.ToBusinessModel();
        business.Id = id;
        await regularOperationService.UpdateAsync(business, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Удалить быструю операцию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await regularOperationService.DeleteAsync(id, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Восстановить быструю операцию по идентификатору.
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
        await regularOperationService.RestoreAsync(id, cancellationToken);
        return Ok();
    }
}
