using Category = Money.Business.Models.Category;
using FastOperation = Money.Business.Models.FastOperation;
using Operation = Money.Business.Models.Operation;
using Place = Money.Business.Models.Place;

namespace Money.Business.Mappers;

public static class MappingExtensions
{
    public static Category Adapt(this Data.Entities.Category model)
    {
        return new()
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Color = model.Color,
            ParentId = model.ParentId,
            Order = model.Order,
            OperationType = (OperationTypes)model.TypeId,
        };
    }

    public static Operation Adapt(this Data.Entities.Operation model, IEnumerable<Place>? dbPlaces = null)
    {
        return new()
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

    public static FastOperation Adapt(this Data.Entities.FastOperation model, IEnumerable<Place>? dbPlaces = null)
    {
        return new()
        {
            CategoryId = model.CategoryId,
            Sum = model.Sum,
            Comment = model.Comment,
            Place = model.PlaceId.HasValue
                ? dbPlaces?.FirstOrDefault(x => x.Id == model.PlaceId)?.Name
                : null,
            Id = model.Id,
            Name = model.Name,
            Order = model.Order,
        };
    }

    public static Place Adapt(this Data.Entities.Place model)
    {
        return new()
        {
            Id = model.Id,
            Name = model.Name,
        };
    }
}
