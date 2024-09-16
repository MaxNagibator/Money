using Microsoft.EntityFrameworkCore;
using Money.Business.Enums;
using Money.Business.Models;
using Money.Common.Exceptions;
using Money.Data;

namespace Money.Business.Services;

public class PaymentCategoryService(RequestEnvironment environment, ApplicationDbContext context)
{
    public async Task<ICollection<PaymentCategory>> Get(PaymentTypes? type = null)
    {
        var dbCats = context.Categories.Where(x => x.UserId == environment.UserId);
        if (type != null)
        {
            dbCats = dbCats.Where(x => x.TypeId == (int)type);
        }

        var dbCategories = await dbCats.OrderBy(x => x.Order == null).ThenBy(x => x.Order).ThenBy(x => x.Name).ToListAsync();
        var categories = new List<PaymentCategory>();
        foreach (var dbCategory in dbCategories)
        {
            var category = new PaymentCategory
            {
                Id = dbCategory.Id,
                Name = dbCategory.Name,
                Description = dbCategory.Description,
                Color = dbCategory.Color,
                ParentId = dbCategory.ParentId,
                Order = dbCategory.Order,
                PaymentType = (PaymentTypes)dbCategory.TypeId
            };
            categories.Add(category);
        }

        return categories;
    }

    public async Task<int> Create(PaymentCategory category)
    {
        if (environment.UserId == null)
        {
            throw new Exception("empty userId");
        }
        if (category.ParentId != null)
        {
            var hasCategory = context.Categories.Any(x => x.UserId == environment.UserId && x.Id == category.ParentId && x.TypeId == ((int)category.PaymentType));
            if (!hasCategory)
            {
                throw new BusinessException("parent category not found");
            }
        }

        //todo need optimization in future
        var categoryId = context.Categories.Where(x => x.UserId == environment.UserId).Select(x => x.Id).DefaultIfEmpty(0).Max() + 1;

        var dbCategory = new Data.Entities.Category
        {
            Id = categoryId,
            UserId = environment.UserId.Value,
            ParentId = category.ParentId,
            Color = category.Color,
            Description = category.Description,
            Name = category.Name,
            Order = category.Order,
            TypeId = (int)category.PaymentType
        };
        await context.Categories.AddAsync(dbCategory);
        context.SaveChanges();
        return categoryId;
    }
}
