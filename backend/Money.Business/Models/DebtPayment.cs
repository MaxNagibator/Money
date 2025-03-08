namespace Money.Business.Models;

/// <summary>
/// Оплата долга.
/// </summary>
public class DebtPayment
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Сумма.
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Дата.
    /// </summary>
    public DateTime Date { get; set; }
}
