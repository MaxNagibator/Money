namespace Money.Web.Models;

public class OperationCategorySum
{
    public required string Name { get; init; }
    public string? Color { get; init; }
    public decimal MainSum { get; init; }
    public decimal TotalSum { get; init; }
    public int? ParentId { get; init; }
    public List<OperationCategorySum>? SubCategories { get; init; }
}
