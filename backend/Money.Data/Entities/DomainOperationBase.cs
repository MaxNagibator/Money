namespace Money.Data.Entities;

public abstract class DomainOperationBase : UserEntity
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
    ///     Идентификатор места.
    /// </summary>
    public int? PlaceId { get; set; }

    /// <summary>
    ///     Флаг, указывающий, что операция была удалена.
    /// </summary>
    public bool IsDeleted { get; set; }
}
