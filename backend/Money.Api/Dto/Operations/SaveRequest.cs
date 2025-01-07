namespace Money.Api.Dto.Operations;

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
    /// Дата.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Преобразует текущую DTO-модель в бизнес-модель.
    /// </summary>
    /// <returns>
    /// Экземпляр <see cref="Business.Models.Operation" />, который представляет бизнес-модель.
    /// </returns>
    public Operation ToBusinessModel()
    {
        return new()
        {
            CategoryId = CategoryId,
            Sum = Sum,
            Comment = Comment,
            Place = Place,
            Date = Date,
        };
    }
}
