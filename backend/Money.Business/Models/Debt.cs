namespace Money.Business.Models;

public class Debt
{
    public int Id { get; set; }

    public DebtTypes Type { get; set; }

    public decimal Sum { get; set; }

    public string? Comment { get; set; }

    public required string OwnerName { get; set; }

    public DateTime Date { get; set; }

    public decimal PaySum { get; set; }

    public string? PayComment { get; set; }

    public DebtStatus Status { get; set; }

    public bool IsDeleted { get; set; }
}
