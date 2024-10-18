namespace Money.Web.Models;

/// <summary>
///     Категория платежа.
/// </summary>
public class Category
{
    /// <summary>
    ///     Идентификатор (может быть null при создании новой категории).
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    ///     Наименование.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Идентификатор родительской категории, если такая существует.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    ///     Порядок отображения в списке.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    ///     Цвет, представленный в виде строки (например, код цвета HEX).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    ///     Идентификатор типа платежа, связанного с категорией.
    /// </summary>
    public required int PaymentTypeId { get; set; }

    /// <summary>
    ///     Удалён.
    /// </summary>
    public bool IsDeleted { get; set; }
}
