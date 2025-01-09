namespace Money.Api.Dto.Debts;

/// <summary>
/// Запрос на оплату долга.
/// </summary>
public class PayRequest
{
    /// <summary>
    /// Сумма.
    /// </summary>
    public decimal Sum { get; init; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// Дата.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Преобразует текущую DTO-модель в бизнес-модель.
    /// </summary>
    /// <returns>
    /// Экземпляр <see cref="Business.Models.Debt" />, который представляет бизнес-модель.
    /// </returns>
    public DebtPayment ToBusinessModel()
    {
        return new()
        {
            Sum = Sum,
            Comment = Comment,
            Date = Date,
        };
    }
}
