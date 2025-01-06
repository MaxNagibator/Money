namespace Money.Web.Models;

/// <summary>
/// Категория операции.
/// </summary>
public class Category
{
    public static readonly Category Empty = new()
    {
        Name = "Несуществующая",
        OperationType = OperationTypes.None,
    };

    /// <summary>
    /// Идентификатор (может быть null при создании новой категории).
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Идентификатор родительской категории, если такая существует.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Порядок отображения в списке.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Цвет, представленный в виде строки (например, код цвета HEX).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Типа операции, связанного с категорией.
    /// </summary>
    public required OperationTypes.Value OperationType { get; set; }

    /// <summary>
    /// Удалён.
    /// </summary>
    public bool IsDeleted { get; set; }
}
