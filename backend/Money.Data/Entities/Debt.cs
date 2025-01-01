namespace Money.Data.Entities;

public class Debt : UserEntity
{
    public DateTime Date { get; set; }

    public decimal Sum { get; set; }

    public int TypeId { get; set; }

    public string? Comment { get; set; }

    public decimal PaySum { get; set; }

    public int StatusId { get; set; }

    public string? PayComment { get; set; }

    public int DebtUserId { get; set; }

    public bool IsDeleted { get; set; }
}
