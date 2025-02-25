using FastOperation = Money.Business.Models.FastOperation;
using Operation = Money.Business.Models.Operation;
using Place = Money.Business.Models.Place;

namespace Money.Business.Mappers;

public static class MappingExtensions
{
    [Obsolete("private GetBusinessModel")]
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

    [Obsolete("private GetBusinessModel")]
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

    [Obsolete("private GetBusinessModel")]
    public static Place Adapt(this Data.Entities.Place model)
    {
        return new()
        {
            Id = model.Id,
            Name = model.Name,
        };
    }
}
