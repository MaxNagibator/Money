using Money.Data.Extensions;

namespace Money.Business.Services;

public class CategoriesService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UsersService usersService)
{
    public async Task<IEnumerable<Category>> GetAsync(int? type = null, CancellationToken cancellationToken = default)
    {
        var query = context.Categories
            .IsUserEntity(environment.UserId);

        if (type != null)
        {
            query = query.Where(x => x.TypeId == type);
        }

        var models = await query
            .OrderBy(x => x.Order == null)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Name)
            .Select(x => GetBusinessModel(x))
            .ToListAsync(cancellationToken);

        return models;
    }

    public async Task<Category> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdInternal(id, cancellationToken);
        return GetBusinessModel(entity);
    }

    public async Task<int> CreateAsync(Category model, CancellationToken cancellationToken = default)
    {
        Validate(model);

        await ValidateParentCategoryAsync(model.ParentId, (int)model.OperationType, cancellationToken);

        var id = await usersService.GetNextCategoryIdAsync(cancellationToken);

        var entity = new Data.Entities.Category
        {
            Id = id,
            UserId = environment.UserId,
            ParentId = model.ParentId,
            Color = model.Color,
            Description = model.Description,
            Name = model.Name,
            Order = model.Order,
            TypeId = (int)model.OperationType,
        };

        await context.Categories.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateAsync(Category model, CancellationToken cancellationToken = default)
    {
        Validate(model);

        var entity = await context.Categories.FirstOrDefaultAsync(environment.UserId, model.Id, cancellationToken)
                     ?? throw new NotFoundException("категория", model.Id);

        await ValidateParentCategoryAsync(model.ParentId, entity.TypeId, cancellationToken);
        await ValidateRecursiveParentingAsync(model.Id, model.ParentId, entity.TypeId, cancellationToken);

        entity.ParentId = model.ParentId;
        entity.Color = model.Color;
        entity.Description = model.Description;
        entity.Name = model.Name;
        entity.Order = model.Order;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdInternal(id, cancellationToken);

        if (await context.Categories.AnyAsync(x => x.ParentId == id && x.UserId == environment.UserId, cancellationToken))
        {
            throw new BusinessException("Извините, но сначала необходимо удалить дочерние категории. Пожалуйста, выполните это действие и попробуйте снова.");
        }

        entity.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetCategory(id);

        if (entity.IsDeleted == false)
        {
            throw new BusinessException("Извините, но невозможно восстановить неудаленную сущность");
        }

        if (entity.ParentId != null)
        {
            var dbParentCategory = await GetCategory(entity.ParentId.Value);

            if (dbParentCategory.IsDeleted)
            {
                throw new BusinessException("Извините, но невозможно восстановить дочернюю категорию у удаленной родительской категории");
            }
        }

        entity.IsDeleted = false;
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
        var id = 1;

        if (isAdd)
        {
            id = await usersService.GetNextCategoryIdAsync(cancellationToken);
        }
        else
        {
            // TODO: Возможно нужно использовать мягкое удаление, но для разработки сделано жесткое.
            // В дальнейшем возможем лучше вообще убрать эту ветку.
            context.Categories.RemoveRange(context.Categories.IgnoreQueryFilters().IsUserEntity(environment.UserId));
        }

        var categories = DatabaseSeeder.SeedCategories(environment.UserId, out var lastIndex, id);
        await usersService.SetNextCategoryIdAsync(lastIndex + 1, cancellationToken);

        await context.Categories.AddRangeAsync(categories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static Category GetBusinessModel(Data.Entities.Category model)
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

    private static void Validate(Category model)
    {
        if (model.Name.Length > 500)
        {
            throw new BusinessException("Извините, название слишком длинное");
        }

        if (Enum.IsDefined(model.OperationType) == false)
        {
            throw new BusinessException("Извините, неподдерживаемый тип категории");
        }

        if (model.Description?.Length > 4000)
        {
            throw new BusinessException("Извините, описание слишком длинное");
        }

        if (model.Color?.Length > 100)
        {
            throw new BusinessException("Извините, цвет слишком длинный");
        }
    }

    private async Task<Data.Entities.Category> GetByIdInternal(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Categories
                         .IsUserEntity(environment.UserId, id)
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException("категория", id);

        return entity;
    }

    private async Task ValidateParentCategoryAsync(int? parentId, int operationType, CancellationToken cancellationToken = default)
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

    private async Task ValidateRecursiveParentingAsync(int categoryId, int? parentId, int typeId, CancellationToken cancellationToken = default)
    {
        var nextParentId = parentId;

        while (nextParentId != null)
        {
            var parent = await context.Categories
                .Where(x => x.TypeId == typeId)
                .FirstOrDefaultAsync(environment.UserId, nextParentId.Value, cancellationToken);

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
