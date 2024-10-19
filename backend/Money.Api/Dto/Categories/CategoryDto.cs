namespace Money.Api.Dto.Categories;

/// <summary>
///     Категория платежа.
/// </summary>
public class CategoryDto
{
    /// <summary>
    ///     Идентификатор.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    ///     Наименование.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Идентификатор родительской категории (если есть).
    /// </summary>
    public int? ParentId { get; init; }

    /// <summary>
    ///     Порядок отображения.
    /// </summary>
    public int? Order { get; init; }

    /// <summary>
    ///     Цвет.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    ///     Идентификатор типа платежа.
    /// </summary>
    public required int PaymentTypeId { get; init; }

    /// <summary>
    ///     Фабричный метод для создания DTO категории на основе бизнес-модели.
    /// </summary>
    /// <param name="category">Бизнес-модель категории.</param>
    /// <returns>Новый объект <see cref="CategoryDto" />.</returns>
    public static CategoryDto FromBusinessModel(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            ParentId = category.ParentId,
            Order = category.Order,
            Color = category.Color,
            PaymentTypeId = category.PaymentType,
        };
    }
}
