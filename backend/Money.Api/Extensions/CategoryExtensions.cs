using Money.Api.Constracts.Categories;
using Money.Business.Enums;

namespace Money.Api.Extensions;

public static class CategoryExtensions
{
    public static CategoryDTO ToCategoryDTO(this Category category)
    {
        return new CategoryDTO
        {
            Id = category.Id,
            Name = category.Name,
            ParentId = category.ParentId,
            Order = category.Order,
            Color = category.Color,
            OperationTypeId = category.OperationType,
        };
    }

    public static Category ToBusinessCategory(this CategoryDetailsDTO category)
    {
        return new Category
        {
            Color = category.Color,
            Name = category.Name,
            Order = category.Order,
            Description = category.Description,
            OperationType = category.OperationTypeId.ToOperationType(),
            ParentId = category.ParentId,
        };
    }

    public static Category ToBusinessCategory(this CategoryDetailsDTO category, int id)
    {
        Category model = ToBusinessCategory(category);
        model.Id = id;
        return model;
    }
}
