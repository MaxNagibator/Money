namespace Money.Web.Models;

public class Category
{
    /// <summary>
    ///     Идентификатор категории.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    ///     Название категории.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Идентификатор родительской категории (если есть).
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    ///     Порядок отображения категории.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    ///     Цвет категории.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    ///     Идентификатор типа платежа.
    /// </summary>
    public required int PaymentTypeId { get; set; }
}
