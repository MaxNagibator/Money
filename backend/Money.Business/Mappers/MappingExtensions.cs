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
            PaymentType = dbCategory.TypeId,
        };
    }

    public static Operation Adapt(this DomainPayment dbPayment, IEnumerable<DomainPlace> dbPlaces)
    {
        return new Operation
        {
            CategoryId = dbPayment.CategoryId,
            Sum = dbPayment.Sum,
            Comment = dbPayment.Comment,
            Place = dbPayment.PlaceId.HasValue
                ? dbPlaces.FirstOrDefault(x => x.Id == dbPayment.PlaceId)?.Name
                : null,
            Id = dbPayment.Id,
            Date = dbPayment.Date,
            CreatedTaskId = dbPayment.CreatedTaskId,
        };
    }
}
