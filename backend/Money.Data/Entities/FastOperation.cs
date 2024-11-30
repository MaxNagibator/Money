namespace Money.Data.Entities;

public class FastOperation : OperationBase
{
    /// <summary>
    ///     Наименование.
    /// </summary>
    [Required]
    [StringLength(500)]
    public required string Name { get; set; }

    /// <summary>
    ///     Значение сортировки.
    /// </summary>
    public int? Order { get; set; }
}
