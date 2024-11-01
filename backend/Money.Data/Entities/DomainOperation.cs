namespace Money.Data.Entities;

public class DomainOperation : UserEntity
{
    /// <summary>
    ///     Сумма.
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    ///     Идентификатор категории.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    ///     Комментарий.
    /// </summary>
    [StringLength(4000)]
    public string? Comment { get; set; }

    /// <summary>
    ///     Дата.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     Идентификатор регулярной задачи.
    /// </summary>
    /// <remarks>
    ///     В одной таблице хранятся две сущности: операции (TaskId=null) и регулярные задачи (TaskId!=null).
    /// </remarks>
    public int? TaskId { get; set; }

    /// <summary>
    ///     Идентификатор родительской регулярной задачи.
    /// </summary>
    /// <remarks>
    ///     Не null, если операция создана регулярной задачей.
    /// </remarks>
    public int? CreatedTaskId { get; set; }

    /// <summary>
    ///     Идентификатор места.
    /// </summary>
    public int? PlaceId { get; set; }

    /// <summary>
    ///     Флаг, указывающий, что операция была удалена.
    /// </summary>
    public bool IsDeleted { get; set; }
}
