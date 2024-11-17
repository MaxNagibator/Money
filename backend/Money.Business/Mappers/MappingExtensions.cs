namespace Money.Business.Mappers;

public static class MappingExtensions
{
    public static Category Adapt(this DomainCategory dbCategory)
    {
        return new Category
        {
            Id = dbCategory.Id,
            Name = dbCategory.Name,
            Description = dbCategory.Description,
            Color = dbCategory.Color,
            ParentId = dbCategory.ParentId,
            Order = dbCategory.Order,
            OperationType = dbCategory.TypeId,
        };
    }

    public static Operation Adapt(this DomainOperation dbOperation, IEnumerable<DomainPlace>? dbPlaces = null)
    {
        return new Operation
        {
            CategoryId = dbOperation.CategoryId,
            Sum = dbOperation.Sum,
            Comment = dbOperation.Comment,
            Place = dbOperation.PlaceId.HasValue
                ? dbPlaces?.FirstOrDefault(x => x.Id == dbOperation.PlaceId)?.Name
                : null,
            Id = dbOperation.Id,
            Date = dbOperation.Date,
            CreatedTaskId = dbOperation.CreatedTaskId,
        };
    }
}
