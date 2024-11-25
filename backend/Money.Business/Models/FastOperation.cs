namespace Money.Business.Models;

/// <summary>
///     Быстрая операция.
/// </summary>
public class FastOperation
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public int? Order { get; set; }

    public required int CategoryId { get; set; }

    public decimal Sum { get; set; }

    public string? Comment { get; set; }

    public string? Place { get; set; }
}
