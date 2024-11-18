namespace Money.Business.Mappers;

public static class MappingExtensions
{
    public static Category Adapt(this DomainCategory model)
    {
        return new Category
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Color = model.Color,
            ParentId = model.ParentId,
            Order = model.Order,
            OperationType = model.TypeId,
        };
    }

    public static Operation Adapt(this DomainOperation model, IEnumerable<Place>? dbPlaces = null)
    {
        return new Operation
        {
            CategoryId = model.CategoryId,
            Sum = model.Sum,
            Comment = model.Comment,
            Place = model.PlaceId.HasValue
                ? dbPlaces?.FirstOrDefault(x => x.Id == model.PlaceId)?.Name
                : null,
            Id = model.Id,
            Date = model.Date,
            CreatedTaskId = model.CreatedTaskId,
        };
    }

    public static Place Adapt(this DomainPlace model)
    {
        return new Place
        {
            Id = model.Id,
            Name = model.Name,
        };
    }
}
