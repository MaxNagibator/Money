namespace Money.Business.Models;

/// <summary>
/// Регулярная операция.
/// </summary>
public class RegularOperation : OperationBase
{
    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    // TODO: потом на регекс переделаем, пока так походит
    /// <summary>
    /// Тип временного шага.
    /// </summary>
    public RegularOperationTimeTypes TimeType { get; set; }

    /// <summary>
    /// Значение временного шага.
    /// </summary>
    public int? TimeValue { get; set; }

    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime DateFrom { get; set; }

    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Время последнего запуска.
    /// </summary>
    public DateTime? RunTime { get; set; }
}
