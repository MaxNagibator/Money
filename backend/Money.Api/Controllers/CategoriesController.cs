using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Categories;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class CategoriesController(CategoryService categoryService) : ControllerBase
{
    /// <summary>
    ///     Получить список категорий операций.
    /// </summary>
    /// <param name="type">Тип категории (опционально).</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Массив категорий операций.</returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(CategoryDto[]), StatusCodes.Status200OK)]
    public async Task<CategoryDto[]> Get([FromQuery] int? type, CancellationToken cancellationToken)
    {
        ICollection<Category> categories = await categoryService.GetAsync(type, cancellationToken);
        return categories.Select(CategoryDto.FromBusinessModel).ToArray();
    }

    /// <summary>
    ///     Получить категорию операции по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Информация о категории.</returns>
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<CategoryDto> GetById(int id, CancellationToken cancellationToken)
    {
        Category category = await categoryService.GetByIdAsync(id, cancellationToken);
        return CategoryDto.FromBusinessModel(category);
    }

    /// <summary>
    ///     Создать новую категорию операции.
    /// </summary>
    /// <param name="request">Данные для создания новой категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Идентификатор созданной категории.</returns>
    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<int> CreateAsync([FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        Category business = request.ToBusinessModel();
        return await categoryService.CreateAsync(business, cancellationToken);
    }

    /// <summary>
    ///     Обновить существующую категорию операции.
    /// </summary>
    /// <param name="id">Идентификатор обновляемой категории.</param>
    /// <param name="request">Данные для обновления категории.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    [HttpPut]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Update(int id, [FromBody] SaveRequest request, CancellationToken cancellationToken)
    {
        Category business = request.ToBusinessModel();
        business.Id = id;
        await categoryService.UpdateAsync(business, cancellationToken);
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
    public async Task Restore(int id, CancellationToken cancellationToken)
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
