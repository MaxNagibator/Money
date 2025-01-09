using Money.Business.Enums;

namespace Money.Api.Dto.Debts;

/// <summary>
/// Запрос на сохранение долга.
/// </summary>
public class SaveRequest
{
    /// <summary>
    /// Идентификатор типа.
    /// </summary>
    public int TypeId { get; init; }

    /// <summary>
    /// Сумма.
    /// </summary>
    public decimal Sum { get; init; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// Имя владельца, которому принадлежит.
    /// </summary>
    public required string OwnerName { get; init; }

    /// <summary>
    /// Дата возникновения.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Преобразует текущую DTO-модель в бизнес-модель.
    /// </summary>
    /// <returns>
    /// Экземпляр <see cref="Business.Models.Debt" />, который представляет бизнес-модель.
    /// </returns>
    public Debt ToBusinessModel()
    {
        return new()
        {
            Sum = Sum,
            Comment = Comment,
            Date = Date,
            OwnerName = OwnerName,
            Type = (DebtTypes)TypeId,
        };
    }
}
