using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Constracts.Categories;
using Money.Api.Extensions;
using Money.Business.Enums;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class CategoriesController(CategoryService categoryService) : ControllerBase, ICategoriesResource
{
    /// <summary>
    ///     Получить список категорий операций.
    /// </summary>
    /// <param name="type">Тип категории (опционально).</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив категорий операций.</returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(CategoryDTO[]), StatusCodes.Status200OK)]
    public async Task<IEnumerable<CategoryDTO>> GetListAsync(
        [FromQuery] string? type = null,
        CancellationToken cancellationToken = default)
    {
        ICollection<Category> categories = await categoryService.GetAsync(type.ToOperationType(), cancellationToken);
        return categories.Select(CategoryExtensions.ToCategoryDTO);
    }

    /// <summary>
    ///     Получить категорию операции по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Информация о категории.</returns>
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(typeof(CategoryDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<CategoryDTO> GetByIdAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        Category category = await categoryService.GetByIdAsync(id, cancellationToken);
        return category.ToCategoryDTO();
    }

    /// <summary>
    ///     Создать новую категорию операции.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Идентификатор созданной категории.</returns>
    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<int> CreateAsync(CategoryDetailsDTO operation, CancellationToken cancellationToken)
    {
        Category business = operation.ToBusinessCategory();
        return categoryService.CreateAsync(business, cancellationToken);
    }

    /// <summary>
    ///     Обновить существующую категорию операции.
    /// </summary>
    /// <param name="id">Идентификатор обновляемой категории.</param>
    /// <param name="operation"></param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPut]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task UpdateAsync(int id, CategoryDetailsDTO operation, CancellationToken cancellationToken)
    {
        Category business = operation.ToBusinessCategory();
        return categoryService.UpdateAsync(business, cancellationToken);
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
        await categoryService.DeleteAsync(id, cancellationToken);
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
        await categoryService.RestoreAsync(id, cancellationToken);
    }

    /// <summary>
    ///     Восстановить категории по умолчанию.
    /// </summary>
    /// <param name="isAdd">
    ///     Флаг, указывающий, добавлять ли категории по умолчанию (по умолчанию true).
    ///     <c>
    ///         Если значение false, все добавленные пользователем категории будут удалены без возможности восстановления!
    ///     </c>
    /// </param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPost]
    [Route("/LoadDefault/{isAdd:bool?}")]
    public async Task LoadDefault(bool? isAdd, CancellationToken cancellationToken)
    {
        await categoryService.LoadDefaultAsync(isAdd ?? true, cancellationToken);
    }
}
