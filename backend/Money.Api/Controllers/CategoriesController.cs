using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Categories;
using Money.Business.Services;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class CategoriesController(CategoryService categoryService) : ControllerBase
{
    /// <summary>
    ///     Получить список категорий платежей.
    /// </summary>
    /// <param name="type">Тип категории (необязательный).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список категорий платежей.</returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(CategoryDto[]), StatusCodes.Status200OK)]
    public async Task<CategoryDto[]> Get([FromQuery] int? type, CancellationToken cancellationToken)
    {
        ICollection<Business.Models.Category> categories = await categoryService.GetAsync(type, cancellationToken);
        return categories.Select(x => new CategoryDto(x)).ToArray();
    }

    /// <summary>
    ///     Получить категорию платежа по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Информация о категории.</returns>
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<CategoryDto> GetById(int id, CancellationToken cancellationToken)
    {
        Business.Models.Category category = await categoryService.GetByIdAsync(id, cancellationToken);
        return new CategoryDto(category);
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
    public async Task<int> CreateAsync([FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        Business.Models.Category business = request.GetBusinessModel();
        int id = await categoryService.CreateAsync(business, cancellationToken);
        return id;
    }

    /// <summary>
    ///     Обновить существующую категорию платежа.
    /// </summary>
    /// <param name="request">Данные для обновление категории.</param>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Сообщение об обновлении.</returns>
    [HttpPut]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task Update([FromBody] SaveRequest request, int id, CancellationToken cancellationToken)
    {
        Business.Models.Category business = request.GetBusinessModel();
        business.Id = id;
        await categoryService.UpdateAsync(business, cancellationToken);
    }

    /// <summary>
    ///     Удалить категорию платежа по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Сообщение об удалении.</returns>
    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Delete(int id, CancellationToken cancellationToken)
    {
        await categoryService.DeleteAsync(id, cancellationToken);
    }

    /// <summary>
    ///     Востановить категорию платежа по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Сообщение об удалении.</returns>
    [HttpPost]
    [Route("{id:int}/Restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Restore(int id, CancellationToken cancellationToken)
    {
        await categoryService.RestoreAsync(id, cancellationToken);
    }
}
