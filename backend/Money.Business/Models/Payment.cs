using Money.Business.Enums;

namespace Money.Business.Models;

public class Payment
{
    public int Id { get; set; }

    public required int CategoryId { get; set; }

    public decimal Sum { get; set; }

    public string? Comment { get; set; }

    public string? Place { get; set; }

    public DateTime Date { get; set; }

    public int? CreatedTaskId { get; set; }
}
