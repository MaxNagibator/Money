using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.CarEvents;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class CarEventsController(CarEventsService service) : ControllerBase
{
    /// <summary>
    /// Получить список авто-событий.
    /// </summary>
    /// <param name="id">Идентификатор авто.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив авто-событий.</returns>
    [HttpGet("Car/{id:int}")]
    [ProducesResponseType(typeof(IEnumerable<CarEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var models = await service.GetAsync(id, cancellationToken);
        return Ok(models.Select(CarEventDto.FromBusinessModel));
    }

    /// <summary>
    /// Получить авто-событие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор авто-события.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Информация об авто-событии.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CarEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var model = await service.GetByIdAsync(id, cancellationToken);
        return Ok(CarEventDto.FromBusinessModel(model));
    }

    /// <summary>
    /// Создать авто-событие.
    /// </summary>
    /// <param name="request">Данные для создания авто-события.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Идентификатор созданного авто-события.</returns>
    [HttpPost("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        var model = request.ToBusinessModel();
        var id = await service.CreateAsync(model, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Обновить авто-события.
    /// </summary>
    /// <param name="id">Идентификатор обновляемого авто-события.</param>
    /// <param name="request">Данные для обновления авто-события.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        var model = request.ToBusinessModel();
        model.Id = id;
        await service.UpdateAsync(model, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Удалить авто-событие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор авто-события.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Восстановить авто-событие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор авто-события.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost("{id:int}/Restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        await service.RestoreAsync(id, cancellationToken);
        return NoContent();
    }
}
