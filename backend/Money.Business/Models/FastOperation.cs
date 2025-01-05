namespace Money.Business.Models;

/// <summary>
/// Быстрая операция.
/// </summary>
public class FastOperation : OperationBase
{
    public required string Name { get; set; }

    public int? Order { get; set; }
}
