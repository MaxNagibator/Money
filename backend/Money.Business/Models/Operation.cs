namespace Money.Business.Models;

/// <summary>
///     Операция.
/// </summary>
public class Operation
{
    public int Id { get; set; }

    public required int CategoryId { get; set; }

    public decimal Sum { get; set; }

    public string? Comment { get; set; }

    public string? Place { get; set; }

    public DateTime Date { get; set; }

    /// <summary>
    ///     Идентификатор родительской регулярной задачи.
    /// </summary>
    /// <remarks>
    ///     Не null, если платеж создан регулярной задачей.
    /// </remarks>
    public int? CreatedTaskId { get; set; }
}
