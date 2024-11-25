namespace Money.Data.Entities;

public class DomainFastOperation : DomainOperationBase
{
    /// <summary>
    ///     Значение сортировки.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }
}
