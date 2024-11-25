namespace Money.Data.Entities;

public class DomainOperation : DomainOperationBase
{
    /// <summary>
    ///     Дата.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     Идентификатор родительской регулярной задачи.
    /// </summary>
    /// <remarks>
    ///     Не null, если операция создана регулярной задачей.
    /// </remarks>
    public int? CreatedTaskId { get; set; }
}
