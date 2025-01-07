namespace Money.Business.Models;

public class DebtPayment
{
    public int Id { get; set; }

    public decimal Sum { get; set; }

    public string? Comment { get; set; }

    public DateTime Date { get; set; }
}
