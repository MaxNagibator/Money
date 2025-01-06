using Money.Business.Mappers;
using Money.Data.Extensions;

namespace Money.Business.Services;

public class CategoryService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService)
{
    public async Task<IEnumerable<Category>> GetAsync(int? type = null, CancellationToken cancellationToken = default)
    {
        var query = context.Categories.IsUserEntity(environment.UserId);

        if (type != null)
        {
            query = query.Where(x => x.TypeId == type);
        }

        var categories = await query
            .OrderBy(x => x.Order == null)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Name)
            .Select(x => x.Adapt())
            .ToListAsync(cancellationToken);

        return categories;
    }

    public async Task<Category> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var dbCategory = await GetByIdInternal(id, cancellationToken);
        return dbCategory.Adapt();
    }

    public async Task<int> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        await ValidateParentCategoryAsync(category.ParentId, (int)category.OperationType, cancellationToken);

        var categoryId = await userService.GetNextCategoryIdAsync(cancellationToken);

        var dbCategory = new Data.Entities.Category
        {
            Id = categoryId,
            UserId = environment.UserId.Value,
            ParentId = category.ParentId,
            Color = category.Color,
            Description = category.Description,
            Name = category.Name,
            Order = category.Order,
            TypeId = (int)category.OperationType,
        };

        await context.Categories.AddAsync(dbCategory, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return categoryId;
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken)
    {
        var dbCategory = await context.Categories.SingleOrDefaultAsync(environment.UserId, category.Id, cancellationToken)
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

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var dbCategory = await GetByIdInternal(id, cancellationToken);

        if (await context.Categories.AnyAsync(x => x.ParentId == id && x.UserId == environment.UserId, cancellationToken))
        {
            throw new BusinessException("Извините, но сначала необходимо удалить дочерние категории. Пожалуйста, выполните это действие и попробуйте снова.");
        }

        dbCategory.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        var dbCategory = await GetCategory(id);

        if (dbCategory.IsDeleted == false)
        {
            throw new BusinessException("Извините, но невозможно восстановить неудаленную категорию");
        }

        if (dbCategory.ParentId != null)
        {
            var dbParentCategory = await GetCategory(dbCategory.ParentId.Value);

            if (dbParentCategory.IsDeleted)
            {
                throw new BusinessException("Извините, но невозможно восстановить дочернюю категорию у удаленной родительской категории");
            }
        }

        dbCategory.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);
        return;

        async Task<Data.Entities.Category> GetCategory(int categoryId)
        {
            var domainCategory = await context.Categories
                .IgnoreQueryFilters()
                .IsUserEntity(environment.UserId, categoryId)
                .FirstOrDefaultAsync(cancellationToken);

            if (domainCategory == null)
            {
                throw new NotFoundException("категория", categoryId);
            }

            return domainCategory;
        }
    }

    public async Task LoadDefaultAsync(bool isAdd = true, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан");
        }

        var categoryId = 1;

        if (isAdd)
        {
            categoryId = await userService.GetNextCategoryIdAsync(cancellationToken);
        }
        else
        {
            // TODO: Возможно нужно использовать мягкое удаление, но для разработки сделано жесткое.
            // В дальнейшем возможем лучше вообще убрать эту ветку.
            context.Categories.RemoveRange(context.Categories.IgnoreQueryFilters().IsUserEntity(environment.UserId));
        }

        var categories = DatabaseSeeder.SeedCategories(environment.UserId.Value, out var lastIndex, categoryId);
        await userService.SetNextCategoryIdAsync(lastIndex + 1, cancellationToken);

        await context.Categories.AddRangeAsync(categories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Data.Entities.Category> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        var dbCategory = await context.Categories
                             .IsUserEntity(environment.UserId, id)
                             .FirstOrDefaultAsync(cancellationToken)
                         ?? throw new NotFoundException("категория", id);

        return dbCategory;
    }

    private async Task ValidateParentCategoryAsync(int? parentId, int operationType, CancellationToken cancellationToken)
    {
        if (parentId == null)
        {
            return;
        }

        var parentExists = await context.Categories
            .IsUserEntity(environment.UserId, parentId)
            .AnyAsync(x => x.TypeId == operationType, cancellationToken);

        if (parentExists == false)
        {
            throw new BusinessException("Извините, но родительская категория не найдена. Пожалуйста, проверьте правильность введенных данных.");
        }
    }

    private async Task ValidateRecursiveParentingAsync(int categoryId, int? parentId, int typeId, CancellationToken cancellationToken)
    {
        var nextParentId = parentId;

        while (nextParentId != null)
        {
            var parent = await context.Categories
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
}
