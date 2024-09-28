using Microsoft.EntityFrameworkCore;
using Money.Business.Enums;
using Money.Common.Exceptions;
using Money.Data;
using Money.Data.Entities;
using Money.Data.Extensions;
using Category = Money.Business.Models.Category;

namespace Money.Business.Services;

public class CategoryService(RequestEnvironment environment, ApplicationDbContext context)
{
    public async Task<ICollection<Category>> GetAsync(PaymentTypes? type = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Data.Entities.Category> query = context.Categories.IsUserEntity(environment.UserId);

        if (type != null)
        {
            query = query.Where(x => x.TypeId == type);
        }

        List<Data.Entities.Category> dbCategories = await query
            .OrderBy(x => x.Order == null)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        List<Category> categories = dbCategories.Select(MapTo).ToList();
        return categories;
    }

    public async Task<Category> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.Category dbCategory = await GetByIdInternal(id, cancellationToken);
        Category category = MapTo(dbCategory);
        return category;
    }

    private async Task<Data.Entities.Category> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        Data.Entities.Category dbCategory = await context.Categories.SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                            ?? throw new NotFoundException($"Категория с ID {id} не найдена");

        return dbCategory;
    }

    private Category MapTo(Data.Entities.Category dbCategory)
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

    public async Task<int> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Не указан идентификатор пользователя");
        }

        await ValidateParentCategoryAsync(category.ParentId, category.PaymentType, cancellationToken);

        DomainUser dbUser = await context.DomainUsers.SingleAsync(x => x.Id == environment.UserId, cancellationToken)
                            ?? throw new BusinessException("Пользователь не найден");

        int categoryId = dbUser.NextCategoryId;
        dbUser.NextCategoryId++; // TODO: обработать конкурентные изменения

        Data.Entities.Category dbCategory = new()
        {
            Id = categoryId,
            UserId = environment.UserId.Value,
            ParentId = category.ParentId,
            Color = category.Color,
            Description = category.Description,
            Name = category.Name,
            Order = category.Order,
            TypeId = category.PaymentType,
        };

        await context.Categories.AddAsync(dbCategory, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return categoryId;
    }

    private async Task ValidateParentCategoryAsync(int? parentId, PaymentTypes paymentType, CancellationToken cancellationToken)
    {
        if (parentId == null)
        {
            return;
        }

        bool parentExists = await context.Categories
            .IsUserEntity(environment.UserId, parentId)
            .AnyAsync(x => x.TypeId == paymentType, cancellationToken);

        if (parentExists == false)
        {
            throw new BusinessException("Родительская категория не найдена");
        }
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken)
    {
        Data.Entities.Category dbCategory = await context.Categories.SingleOrDefaultAsync(environment.UserId, category.Id, cancellationToken)
                                            ?? throw new BusinessException($"Категория с ID {category.Id} не найдена");

        await ValidateParentCategoryAsync(category.ParentId, dbCategory.TypeId, cancellationToken);
        await ValidateRecursiveParentingAsync(category.Id, category.ParentId, dbCategory.TypeId, cancellationToken);

        dbCategory.ParentId = category.ParentId;
        dbCategory.Color = category.Color;
        dbCategory.Description = category.Description;
        dbCategory.Name = category.Name;
        dbCategory.Order = category.Order;

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateRecursiveParentingAsync(int categoryId, int? parentId, PaymentTypes typeId, CancellationToken cancellationToken)
    {
        int? nextParentId = parentId;

        while (nextParentId != null)
        {
            Data.Entities.Category? parent = await context.Categories
                .Where(x => x.TypeId == typeId)
                .SingleOrDefaultAsync(environment.UserId, nextParentId.Value, cancellationToken);

            if (parent == null)
            {
                break;
            }

            nextParentId = parent.ParentId;

            if (nextParentId == categoryId)
            {
                throw new BusinessException("Обнаружена рекурсивная зависимость родительских категорий");
            }
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Data.Entities.Category dbCategory = await GetByIdInternal(id, cancellationToken);

        if (await context.Categories.AnyAsync(x => x.ParentId == id && x.UserId == environment.UserId, cancellationToken))
        {
            throw new BusinessException("Сначала удалите дочерние категории");
        }

        dbCategory.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        Data.Entities.Category dbCategory = await context.Categories.IgnoreQueryFilters()
                                                .Where(x => x.IsDeleted)
                                                .SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                            ?? throw new NotFoundException("Категория не найдена");

        if (dbCategory.ParentId != null)
        {
            await GetByIdInternal(dbCategory.ParentId.Value, cancellationToken);
        }

        dbCategory.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);
    }
}
