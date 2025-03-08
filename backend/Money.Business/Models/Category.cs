namespace Money.Business.Models;

/// <summary>
/// Категория операции.
/// </summary>
public class Category
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public required string Name { get; set; }

    // TODO: Удалить на всех уровнях
    public string? Description { get; set; }

    /// <summary>
    /// Идентификатор родительской категории (если есть).
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Порядок отображения.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Цвет.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Тип операции.
    /// </summary>
    public required OperationTypes OperationType { get; set; }
}
