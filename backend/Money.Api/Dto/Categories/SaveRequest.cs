namespace Money.Api.Dto.Categories;

/// <summary>
///     Запрос на сохранение категории платежа.
/// </summary>
public class SaveRequest
{
    /// <summary>
    ///     Цвет категории платежа.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    ///     Идентификатор родительской категории (если есть).
    /// </summary>
    public int? ParentId { get; init; }

    /// <summary>
    ///     Название категории платежа.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Порядок отображения категории.
    /// </summary>
    public int? Order { get; init; }

    /// <summary>
    ///     Описание категории платежа.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Идентификатор типа платежа.
    /// </summary>
    public required int PaymentTypeId { get; init; }

    /// <summary>
    ///     Преобразует текущую DTO-модель в бизнес-модель категории.
    /// </summary>
    /// <returns>
    ///     Экземпляр <see cref="Business.Models.Category" />, который представляет бизнес-модель категории платежа.
    /// </returns>
    public Business.Models.Category ToBusinessModel()
    {
        return new Business.Models.Category
        {
            Color = Color,
            Name = Name,
            Order = Order,
            Description = Description,
            PaymentType = PaymentTypeId,
            ParentId = ParentId,
        };
    }
}
