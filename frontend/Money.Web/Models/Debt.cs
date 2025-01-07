namespace Money.Web.Models;

/// <summary>
/// Долг.
/// </summary>
public class Debt
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int? Id { get; set; }

    public required DebtTypes.Value Type { get; set; }

    public decimal Sum { get; set; }

    public string? Comment { get; set; }

    public required string OwnerName { get; set; }

    public DateTime Date { get; set; }

    public decimal PaySum { get; set; }

    public string? PayComment { get; set; }

    public bool IsDeleted { get; set; }
}
