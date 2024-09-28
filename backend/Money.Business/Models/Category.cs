using Money.Business.Enums;

namespace Money.Business.Models;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public int? Order { get; set; }
    public string? Color { get; set; }
    public required PaymentTypes PaymentType { get; set; }
}
