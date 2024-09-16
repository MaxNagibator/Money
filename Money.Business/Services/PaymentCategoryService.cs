using Microsoft.EntityFrameworkCore;
using Money.Business.Enums;
using Money.Business.Models;
using Money.Common.Exceptions;
using Money.Data;
using Money.Data.Entities;

namespace Money.Business.Services;

public class PaymentCategoryService(RequestEnvironment environment, ApplicationDbContext context)
{
    public async Task<ICollection<PaymentCategory>> GetAsync(PaymentTypes? type = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = context.Categories.Where(x => x.UserId == environment.UserId);

        if (type != null)
        {
            query = query.Where(x => x.TypeId == type);
        }

        List<Category> dbCategories = await query.OrderBy(x => x.Order == null)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        List<PaymentCategory> categories = dbCategories.Select(dbCategory => new PaymentCategory
            {
                Id = dbCategory.Id,
                Name = dbCategory.Name,
                Description = dbCategory.Description,
                Color = dbCategory.Color,
                ParentId = dbCategory.ParentId,
                Order = dbCategory.Order,
                PaymentType = dbCategory.TypeId
            })
            .ToList();

        return categories;
    }

    public async Task<int> CreateAsync(PaymentCategory category, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new Exception("empty userId");
        }

        if (category.ParentId != null)
        {
            bool hasCategory = await context.Categories.AnyAsync(x => x.UserId == environment.UserId && x.Id == category.ParentId && x.TypeId == category.PaymentType, cancellationToken);

            if (hasCategory == false)
            {
                throw new BusinessException("parent category not found");
            }
        }

        //todo need optimization in future
        //+ дополнительный костыль, чтобы запрос заработал
        //(The LINQ expression 'DbSet<Category>().Where(x => x.UserId == __userId_0).Select(x => x.Id).DefaultIfEmpty(__p_1)' could not be translated.)
        int categoryId = context.Categories.AsEnumerable()
                             .Where(x => x.UserId == environment.UserId)
                             .Select(x => x.Id)
                             .DefaultIfEmpty(0)
                             .Max()
                         + 1;

        Category dbCategory = new()
        {
            Id = categoryId,
            UserId = environment.UserId.Value,
            ParentId = category.ParentId,
            Color = category.Color,
            Description = category.Description,
            Name = category.Name,
            Order = category.Order,
            TypeId = category.PaymentType
        };

        await context.Categories.AddAsync(dbCategory, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return categoryId;
    }
}
