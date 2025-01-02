namespace Money.Business.Models;

/// <summary>
/// Фильтр для операций.
/// </summary>
public class OperationFilter
{
    /// <summary>
    /// Дата начала периода.
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Дата окончания периода.
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Список идентификаторов категорий.
    /// </summary>
    public List<int>? CategoryIds { get; set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Место.
    /// </summary>
    public string? Place { get; set; }
}
