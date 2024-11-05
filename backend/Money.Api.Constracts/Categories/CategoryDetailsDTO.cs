namespace Money.Api.Constracts.Categories
{
    public class CategoryDetailsDTO
    {
        public required string Name { get; set; }
        public string? Description { get; init; }
        public required int OperationTypeId { get; set; }
        public int? ParentId { get; set; }
        public int? Order { get; set; }
        public string? Color { get; set; }
    }
}