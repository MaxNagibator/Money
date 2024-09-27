using Microsoft.EntityFrameworkCore;
using Money.Business.Enums;
using Money.Business.Models;
using Money.Common.Exceptions;
using Money.Data;
using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Business.Services;

public class PaymentCategoryService(RequestEnvironment environment, ApplicationDbContext context)
{
    public async Task<ICollection<PaymentCategory>> GetAsync(PaymentTypes? type = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = context.Categories.IsUserEntity(environment.UserId);

        if (type != null)
        {
            query = query.Where(x => x.TypeId == type);
        }

        List<Category> dbCategories = await query.OrderBy(x => x.Order == null)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        List<PaymentCategory> categories = dbCategories.Select(MapTo)
            .ToList();

        return categories;
    }

    public async Task<PaymentCategory> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Category dbCategory = await GetByIdInternal(id, cancellationToken);
        PaymentCategory category = MapTo(dbCategory);
        return category;
    }

    private async Task<Category> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        Category dbCategory = await context.Categories.SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                              ?? throw new NotFoundException("Категория не найдена");

        return dbCategory;
    }

    private PaymentCategory MapTo(Category dbCategory)
    {
        return new PaymentCategory
        {
            Id = dbCategory.Id,
            Name = dbCategory.Name,
            Description = dbCategory.Description,
            Color = dbCategory.Color,
            ParentId = dbCategory.ParentId,
            Order = dbCategory.Order,
            PaymentType = dbCategory.TypeId
        };
    }

    public async Task<int> CreateAsync(PaymentCategory category, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new Exception("empty userId");
        }

        if (category.ParentId != null)
        {
            bool hasCategory = await context.Categories
                .IsUserEntity(environment.UserId, category.ParentId)
                .AnyAsync(x => x.TypeId == category.PaymentType, cancellationToken);

            if (hasCategory == false)
            {
                throw new BusinessException("parent category not found");
            }
        }

        var dbUser = context.DomainUsers.Single(x => x.Id == environment.UserId);
        int categoryId = dbUser.NextCategoryId;
        dbUser.NextCategoryId++; // todo обработать канкаренси

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

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Category dbCategory = await GetByIdInternal(id, cancellationToken);
        if (context.Categories.Any(x => x.ParentId == id && x.UserId == environment.UserId))
        {
            throw new BusinessException("удалите сначала дочернии категории");
        }
        dbCategory.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
    }
}
