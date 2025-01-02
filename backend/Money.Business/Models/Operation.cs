namespace Money.Business.Models;

/// <summary>
/// Операция.
/// </summary>
public class Operation : OperationBase
{
    public DateTime Date { get; set; }

    /// <summary>
    /// Идентификатор родительской регулярной задачи.
    /// </summary>
    /// <remarks>
    /// Не null, если операция создана регулярной задачей.
    /// </remarks>
    public int? CreatedTaskId { get; set; }

    /// <summary>
    /// Идентификатор места.
    /// </summary>
    public int? PlaceId { get; set; }
}
