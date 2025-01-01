using System.ComponentModel;

namespace Money.Business.Models;

public class Debt
{
    public int Id { get; set; }

    public DebtTypes Type { get; set; }

    public decimal Sum { get; set; }

    public string? Comment { get; set; }

    public required string DebtUserName { get; set; }

    public DateTime Date { get; set; }

    public decimal PaySum { get; set; }

    public string? PayComment { get; set; }

    public DebtStatus Status { get; set; }

}

public enum DebtTypes
{
    /// <summary>
    /// Нужно забрать.
    /// </summary>
    [Description("Нужно забрать")]
    Plus = 1,

    /// <summary>
    /// Нужно отдать.
    /// </summary>
    [Description("Нужно отдать")]
    Minus = 2,
}

public enum DebtStatus
{
    [Description("Актуальный")]
    Actual = 1,

    [Description("Уплачен")]
    Paid = 2,
}
