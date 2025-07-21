using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Debts;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class DebtsController(DebtsService service) : ControllerBase
{
    /// <summary>
    /// Получить список долгов.
    /// </summary>
    /// <param name="withPaid">Включить погашенные долги в результат.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив долгов.</returns>
    [HttpGet("")]
    [ProducesResponseType(typeof(IEnumerable<DebtDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken, bool withPaid = false)
    {
        var models = await service.GetAsync(withPaid, cancellationToken);
        return Ok(models.Select(DebtDto.FromBusinessModel));
    }

    /// <summary>
    /// Получить долг по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Информация о долге.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DebtDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var model = await service.GetByIdAsync(id, cancellationToken);
        return Ok(DebtDto.FromBusinessModel(model));
    }

    /// <summary>
    /// Создать долг.
    /// </summary>
    /// <param name="request">Данные для создания долга.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Идентификатор созданного долга.</returns>
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
    /// Обновить долг.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <param name="request">Данные для обновления долга.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    /// Удалить долг по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Восстановить долг по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
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
    /// Оплатить долг.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <param name="request">Данные для обновления операции.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost("{id:int}/Pay")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pay(int id, [FromBody] PayRequest request, CancellationToken cancellationToken)
    {
        var model = request.ToBusinessModel();
        model.Id = id;
        await service.PayAsync(model, cancellationToken);
        return NoContent();
    }

    // TODO: Подумать над with
    /// <summary>
    /// Перенести все долги из одного держателя в другого.
    /// </summary>
    /// <param name="fromUserId">Идентификатор сливаемого должника.</param>
    /// <param name="toUserId">Идентификатор поглощающего должника.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost("MergeOwners/{fromUserId:int}/with/{toUserId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MergeOwners(int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        await service.MergeOwnersAsync(fromUserId, toUserId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Получить список держателей долга.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив держателей долга.</returns>
    [HttpGet("Owners")]
    [ProducesResponseType(typeof(IEnumerable<DebtOwnerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOwners(CancellationToken cancellationToken)
    {
        var owners = await service.GetOwnersAsync(cancellationToken);
        return Ok(owners.Select(DebtOwnerDto.FromBusinessModel));
    }

    /// <summary>
    /// Простить долг.
    /// </summary>
    /// <param name="request">Данные для прощения долга.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost("Forgive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Forgive([FromBody] ForgiveRequest request, CancellationToken cancellationToken)
    {
        await service.ForgiveAsync(request.DebtIds, request.OperationCategoryId, request.OperationComment, cancellationToken);
        return NoContent();
    }
}
