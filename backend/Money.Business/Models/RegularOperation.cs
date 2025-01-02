namespace Money.Business.Models;

/// <summary>
/// Регулярная операция.
/// </summary>
public class RegularOperation : OperationBase
{
    public required string Name { get; set; }

    // todo потом на регекс переделаем, пока так походит
    public RegularOperationTimeTypes TimeType { get; set; }

    public int? TimeValue { get; set; }

    public DateTime DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public DateTime? RunTime { get; set; }
}
