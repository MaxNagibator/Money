namespace Money.Web.Models;

public class RegularOperation
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Категория.
    /// </summary>
    public required Category Category { get; set; }

    /// <summary>
    /// Сумма.
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Место.
    /// </summary>
    public string? Place { get; set; }

    public required RegularOperationTimeTypes.Value TimeType { get; set; }

    public int? TimeValue { get; set; }

    public DateTime DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public DateTime? RunTime { get; set; }

    /// <summary>
    /// Удалён.
    /// </summary>
    public bool IsDeleted { get; set; }
}
