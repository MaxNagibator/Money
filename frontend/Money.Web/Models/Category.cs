namespace Money.Web.Models;

public class Category
{
    /// <summary>
    ///     Идентификатор категории (может быть null при создании новой категории).
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    ///     Название категории.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Идентификатор родительской категории, если такая существует.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    ///     Порядок отображения категории в списке.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    ///     Цвет категории, представленный в виде строки (например, код цвета HEX).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    ///     Идентификатор типа платежа, связанного с категорией.
    /// </summary>
    public required int PaymentTypeId { get; set; }

    /// <summary>
    ///     Была ли категория удалена.
    /// </summary>
    public bool IsDeleted { get; set; }
}
