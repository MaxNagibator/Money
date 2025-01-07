namespace Money.Api.Dto.Categories;

/// <summary>
/// Категория операции.
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Идентификатор родительской категории (если есть).
    /// </summary>
    public int? ParentId { get; init; }

    /// <summary>
    /// Порядок отображения.
    /// </summary>
    public int? Order { get; init; }

    /// <summary>
    /// Цвет.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Идентификатор типа операции.
    /// </summary>
    public required int OperationTypeId { get; init; }

    /// <summary>
    /// Фабричный метод для создания DTO категории на основе бизнес-модели.
    /// </summary>
    /// <param name="category">Бизнес-модель категории.</param>
    /// <returns>Новый объект <see cref="CategoryDto" />.</returns>
    public static CategoryDto FromBusinessModel(Category category)
    {
        return new()
        {
            Id = category.Id,
            Name = category.Name,
            ParentId = category.ParentId,
            Order = category.Order,
            Color = category.Color,
            OperationTypeId = (int)category.OperationType,
        };
    }
}
