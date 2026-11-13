namespace Money.Business.Models;

/// <summary>
/// Движение по долговому счёту.
/// </summary>
public class DebtMovement
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Тип.
    /// </summary>
    public DebtTypes Type { get; set; }

    /// <summary>
    /// Сумма.
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Имя держателя.
    /// </summary>
    public required string OwnerName { get; set; }

    /// <summary>
    /// Дата возникновения.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Удален.
    /// </summary>
    public bool IsDeleted { get; set; }
}
