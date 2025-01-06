namespace Money.Web.Models;

public class Operation
{
    public int? Id { get; set; }

    public required Category Category { get; set; }

    public decimal Sum { get; set; }

    public string? Comment { get; set; }

    public string? Place { get; set; }

    public DateTime Date { get; set; }

    /// <summary>
    /// Идентификатор родительской регулярной задачи.
    /// </summary>
    /// <remarks>
    /// Не null, если операция создана регулярной задачей.
    /// </remarks>
    public int? CreatedTaskId { get; set; }

    /// <summary>
    /// Удалён.
    /// </summary>
    public bool IsDeleted { get; set; }
}
