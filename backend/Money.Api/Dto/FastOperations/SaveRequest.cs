namespace Money.Api.Dto.FastOperations;

/// <summary>
/// Запрос на сохранение операции.
/// </summary>
public class SaveRequest
{
    /// <summary>
    /// Идентификатор категории.
    /// </summary>
    public required int CategoryId { get; set; }

    /// <summary>
    /// Сумма.
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Место.
    /// </summary>
    public string? Place { get; set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Значение сортировки.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Преобразует текущую DTO-модель в бизнес-модель.
    /// </summary>
    /// <returns>
    /// Экземпляр <see cref="Business.Models.FastOperation" />, который представляет бизнес-модель.
    /// </returns>
    public FastOperation ToBusinessModel()
    {
        return new()
        {
            Sum = Sum,
            Name = Name,
            Order = Order,
            CategoryId = CategoryId,
            Comment = Comment,
            Place = Place,
        };
    }
}
