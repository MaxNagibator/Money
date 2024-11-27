namespace Money.Data.Entities;

public class RegularOperation : OperationBase
{
    /// <summary>
    /// Наименование.
    /// </summary>
    [Required]
    [StringLength(500)]
    public required string Name { get; set; }

    //public int TypeId { get; set; }

    //public int TimeId { get; set; }

    //public int? TimeValue { get; set; }

    //[Column(TypeName = "date")]
    //public DateTime DateFrom { get; set; }

    //[Column(TypeName = "date")]
    //public DateTime? DateTo { get; set; }

    //public DateTime? RunTime { get; set; }
}
