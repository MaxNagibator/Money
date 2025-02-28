using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Categories;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class CategoriesController(CategoriesService service) : ControllerBase
{
    /// <summary>
    /// Получить список категорий операций.
    /// </summary>
    /// <param name="type">Тип категории (опционально).</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив категорий операций.</returns>
    [HttpGet("")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([FromQuery] int? type, CancellationToken cancellationToken)
    {
        var models = await service.GetAsync(type, cancellationToken);
        return Ok(models.Select(CategoryDto.FromBusinessModel));
    }

    /// <summary>
    /// Получить категорию операции по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Информация о категории.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var model = await service.GetByIdAsync(id, cancellationToken);
        return Ok(CategoryDto.FromBusinessModel(model));
    }

    /// <summary>
    /// Создать новую категорию операции.
    /// </summary>
    /// <param name="request">Данные для создания новой категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Идентификатор созданной категории.</returns>
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
    /// Обновить существующую категорию операции.
    /// </summary>
    /// <param name="id">Идентификатор обновляемой категории.</param>
    /// <param name="request">Данные для обновления категории.</param>
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
    /// Удалить категорию операции по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
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
    /// Восстановить категорию операции по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
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

    /// <summary>
    /// Восстановить категории по умолчанию.
    /// </summary>
    /// <param name="isAdd">
    /// Флаг, указывающий, добавлять ли категории по умолчанию (по умолчанию true).
    /// <c>
    /// Если значение false, все добавленные пользователем категории будут удалены без возможности восстановления!
    /// </c>
    /// </param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost("/LoadDefault/{isAdd:bool?}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoadDefault(bool? isAdd, CancellationToken cancellationToken)
    {
        await service.LoadDefaultAsync(isAdd ?? true, cancellationToken);
        return NoContent();
    }
}
