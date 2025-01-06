namespace Money.Web.Models;

public class FastOperation
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

    /// <summary>
    /// Приоритет сортировки.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Удалён.
    /// </summary>
    public bool IsDeleted { get; set; }
}
