using System.ComponentModel.DataAnnotations;
using Money.Business.Enums;

namespace Money.Api.Dto.Debts;

/// <summary>
/// Запрос на групповой возврат долгов держателя.
/// </summary>
public class PayOwnerRequest
{
    /// <summary>
    /// Сумма.
    /// </summary>
    public required decimal Sum { get; init; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// Дата.
    /// </summary>
    public required DateTime Date { get; init; }

    /// <summary>
    /// Ожидаемое имя держателя долга.
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public required string OwnerName { get; init; }

    /// <summary>
    /// Ожидаемый идентификатор типа долга.
    /// </summary>
    public required int TypeId { get; init; }

    /// <summary>
    /// Преобразует текущую DTO-модель в бизнес-модель.
    /// </summary>
    /// <returns>
    /// Экземпляр <see cref="Business.Models.DebtOwnerPayment" />, который представляет бизнес-модель.
    /// </returns>
    public DebtOwnerPayment ToBusinessModel()
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
