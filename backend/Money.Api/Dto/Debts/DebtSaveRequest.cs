namespace Money.Api.Dto.Debts;

/// <summary>
///     Запрос на сохранение категории операции.
/// </summary>
public class DebtSaveRequest
{
    public int TypeId { get; init; }

    public decimal Sum { get; init; }

    public string? Comment { get; init; }

    public required string DebtUserName { get; init; }

    public DateTime Date { get; init; }

    /// <summary>
    ///     Преобразует текущую DTO-модель в бизнес-модель.
    /// </summary>
    /// <returns>
    ///     Экземпляр <see cref="Business.Models.Debt" />, который представляет бизнес-модель.
    /// </returns>
    public Debt ToBusinessModel()
    {
        return new Debt
        {
            Sum = Sum,
            Comment = Comment,
            Date = Date,
            DebtUserName = DebtUserName,
            Type = (DebtTypes)TypeId,
        };
    }
}
