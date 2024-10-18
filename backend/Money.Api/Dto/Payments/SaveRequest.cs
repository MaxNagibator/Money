namespace Money.Api.Dto.Payments;

/// <summary>
///     Запрос на сохранение платежа.
/// </summary>
public class SaveRequest
{
    /// <summary>
    ///     Идентификатор категории.
    /// </summary>
    public required int CategoryId { get; set; }

    /// <summary>
    ///     Сумма.
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    ///     Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    ///     Место.
    /// </summary>
    public string? Place { get; set; }

    /// <summary>
    ///     Дата.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     Преобразует текущую DTO-модель в бизнес-модель.
    /// </summary>
    /// <returns>
    ///     Экземпляр <see cref="Business.Models.Payment" />, который представляет бизнес-модель платежа.
    /// </returns>
    public Payment ToBusinessModel()
    {
        return new Payment
        {
            CategoryId = CategoryId,
            Sum = Sum,
            Comment = Comment,
            Place = Place,
            Date = Date,
        };
    }
}
