using Money.Business.Enums;

namespace Money.Api.Dto.Categories;

/// <summary>
/// Запрос на сохранение категории операции.
/// </summary>
public class SaveRequest
{
    /// <summary>
    /// Цвет категории операции.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Идентификатор родительской категории (если есть).
    /// </summary>
    public int? ParentId { get; init; }

    /// <summary>
    /// Название категории операции.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Порядок отображения категории.
    /// </summary>
    public int? Order { get; init; }

    /// <summary>
    /// Описание категории операции.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Идентификатор типа операции.
    /// </summary>
    public required int OperationTypeId { get; init; }

    /// <summary>
    /// Преобразует текущую DTO-модель в бизнес-модель.
    /// </summary>
    /// <returns>
    /// Экземпляр <see cref="Business.Models.Category" />, который представляет бизнес-модель.
    /// </returns>
    public Category ToBusinessModel()
    {
        return new()
        {
            Color = Color,
            Name = Name,
            Order = Order,
            Description = Description,
            OperationType = (OperationTypes)OperationTypeId,
            ParentId = ParentId,
        };
    }
}
