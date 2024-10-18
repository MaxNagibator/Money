using Money.Business.Mappers;
using Money.Data.Extensions;

namespace Money.Business.Services;

public class CategoryService(RequestEnvironment environment, ApplicationDbContext context)
{
    public async Task<ICollection<Category>> GetAsync(PaymentTypes? type = null, CancellationToken cancellationToken = default)
    {
        IQueryable<DomainCategory> query = context.Categories.IsUserEntity(environment.UserId);

        if (type != null)
        {
            query = query.Where(x => x.TypeId == type);
        }

        List<DomainCategory> dbCategories = await query
            .OrderBy(x => x.Order == null)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        List<Category> categories = dbCategories.Select(x => x.Adapt()).ToList();
        return categories;
    }

    public async Task<Category> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        DomainCategory dbCategory = await GetByIdInternal(id, cancellationToken);
        Category category = dbCategory.Adapt();
        return category;
    }

    private async Task<DomainCategory> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        DomainCategory dbCategory = await context.Categories.SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                    ?? throw new NotFoundException("категория", id);

        return dbCategory;
    }

    public async Task<int> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        await ValidateParentCategoryAsync(category.ParentId, category.PaymentType, cancellationToken);

        DomainUser dbUser = await context.DomainUsers.SingleAsync(x => x.Id == environment.UserId, cancellationToken)
                            ?? throw new BusinessException("Извините, но пользователь не найден.");

        int categoryId = dbUser.NextCategoryId;
        dbUser.NextCategoryId++;

        DomainCategory dbCategory = new()
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
            throw new BusinessException("Извините, но родительская категория не найдена. Пожалуйста, проверьте правильность введенных данных.");
        }
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken)
    {
        DomainCategory dbCategory = await context.Categories.SingleOrDefaultAsync(environment.UserId, category.Id, cancellationToken)
                                    ?? throw new NotFoundException("категория", category.Id);

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
            DomainCategory? parent = await context.Categories
                .Where(x => x.TypeId == typeId)
                .SingleOrDefaultAsync(environment.UserId, nextParentId.Value, cancellationToken);

            if (parent == null)
            {
                break;
            }

            nextParentId = parent.ParentId;

            if (nextParentId == categoryId)
            {
                throw new BusinessException("Извините, но обнаружена рекурсивная зависимость родительских категорий. Пожалуйста, проверьте структуру категорий и внесите необходимые изменения.");
            }
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        DomainCategory dbCategory = await GetByIdInternal(id, cancellationToken);

        if (await context.Categories.AnyAsync(x => x.ParentId == id && x.UserId == environment.UserId, cancellationToken))
        {
            throw new BusinessException("Извините, но сначала необходимо удалить дочерние категории. Пожалуйста, выполните это действие и попробуйте снова.");
        }

        dbCategory.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        DomainCategory dbCategory = await context.Categories.IgnoreQueryFilters()
                                        .Where(x => x.IsDeleted)
                                        .SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                    ?? throw new NotFoundException("категория", id);

        if (dbCategory.ParentId != null)
        {
            await GetByIdInternal(dbCategory.ParentId.Value, cancellationToken);
        }

        dbCategory.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreDefaultAsync(bool isAdd = true, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        int categoryId = 1;

        DomainUser dbUser = await context.DomainUsers.SingleAsync(x => x.Id == environment.UserId, cancellationToken)
                            ?? throw new BusinessException("Извините, но пользователь не найден.");

        if (isAdd)
        {
            categoryId = dbUser.NextCategoryId;
        }
        else
        {
            // TODO: Возможно нужно использовать мягкое удаление, но для разработки сделано жесткое.
            // В дальнейшем возможем лучше вообще убрать эту ветку.
            context.Categories.RemoveRange(context.Categories.IsUserEntity(environment.UserId));
        }

        List<DomainCategory> categories = DatabaseSeeder.SeedCategories(environment.UserId.Value, categoryId);
        dbUser.NextCategoryId = categories.Last().Id + 1;

        await context.Categories.AddRangeAsync(categories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
