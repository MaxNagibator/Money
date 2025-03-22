namespace Money.Business.Models;

/// <summary>
/// Быстрая операция.
/// </summary>
public class FastOperation : OperationBase
{
    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Порядок сортировки.
    /// </summary>
    public int? Order { get; set; }
}
