namespace Money.Data.Entities;

public class RegularOperation : OperationBase
{
    /// <summary>
    /// Наименование.
    /// </summary>
    [Required]
    [StringLength(500)]
    public required string Name { get; set; }

    public int TimeTypeId { get; set; }

    public int? TimeValue { get; set; }

    public DateTime DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public DateTime? RunTime { get; set; }
}
