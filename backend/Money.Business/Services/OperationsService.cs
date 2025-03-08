using Money.Data.Extensions;

namespace Money.Business.Services;

public class OperationsService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UsersService usersService,
    CategoriesService categoriesService,
    PlacesService placesService)
{
    public async Task<IEnumerable<Operation>> GetAsync(OperationFilter filter, CancellationToken cancellationToken = default)
    {
        var filteredOperations = FilterOperations(filter);

        var placeIds = await filteredOperations
            .Where(x => x.PlaceId != null)
            .Select(x => x.PlaceId!.Value)
            .ToListAsync(cancellationToken);

        var places = await placesService.GetPlacesAsync(placeIds, cancellationToken);

        var models = await filteredOperations
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        return models.Select(x => GetBusinessModel(x, places)).ToList();
    }

    public async Task<Operation> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdInternal(id, cancellationToken);

        List<Place>? places = null;

        if (entity.PlaceId != null)
        {
            places = await placesService.GetPlacesAsync([entity.PlaceId.Value], cancellationToken);
        }

        return GetBusinessModel(entity, places);
    }

    public async Task<int> CreateAsync(Operation model, CancellationToken cancellationToken = default)
    {
        Validate(model);

        var category = await categoriesService.GetByIdAsync(model.CategoryId, cancellationToken);
        var operationId = await usersService.GetNextOperationIdAsync(cancellationToken);
        var placeId = model.PlaceId ?? await placesService.GetPlaceIdAsync(model.Place, cancellationToken);

        var entity = new Data.Entities.Operation
        {
            Id = operationId,
            UserId = environment.UserId,
            CategoryId = category.Id,
            Sum = model.Sum,
            Comment = model.Comment,
            Date = model.Date,
            PlaceId = placeId,
            CreatedTaskId = model.CreatedTaskId,
        };

        await context.Operations.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return operationId;
    }

    public async Task UpdateAsync(Operation model, CancellationToken cancellationToken = default)
    {
        Validate(model);

        var entity = await context.Operations.FirstOrDefaultAsync(environment.UserId, model.Id, cancellationToken)
                     ?? throw new NotFoundException("операция", model.Id);

        var category = await categoriesService.GetByIdAsync(model.CategoryId, cancellationToken);
        var placeId = await placesService.GetPlaceIdAsync(model.Place, entity, cancellationToken);

        entity.Sum = model.Sum;
        entity.Comment = model.Comment;
        entity.Date = model.Date;
        entity.CategoryId = category.Id;
        entity.PlaceId = placeId;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdInternal(id, cancellationToken);
        entity.IsDeleted = true;
        await placesService.CheckRemovePlaceAsync(entity.PlaceId, entity.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Operations
                         .IgnoreQueryFilters()
                         .Where(x => x.IsDeleted)
                         .FirstOrDefaultAsync(environment.UserId, id, cancellationToken)
                     ?? throw new NotFoundException("операция", id);

        entity.IsDeleted = false;
        await placesService.CheckRestorePlaceAsync(entity.PlaceId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Operation>> UpdateBatchAsync(List<int> operationIds, int categoryId, CancellationToken cancellationToken = default)
    {
        var category = await categoriesService.GetByIdAsync(categoryId, cancellationToken);

        var entities = await context.Operations
            .IsUserEntity(environment.UserId)
            .Where(x => operationIds.Contains(x.Id))
            .Include(x => x.Category)
            .ToListAsync(cancellationToken);

        if (entities.Count != operationIds.Count)
        {
            throw new BusinessException("Одна или несколько операций не найдены");
        }

        foreach (var model in entities)
        {
            model.CategoryId = category.Id;

            if (model.Category?.TypeId != (int)category.OperationType)
            {
                model.Sum = -model.Sum;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return entities.Select(x => GetBusinessModel(x)).ToList();
    }

    private static Operation GetBusinessModel(Data.Entities.Operation model, IEnumerable<Place>? dbPlaces = null)
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

    private static void Validate(Operation model)
    {
        if (model.Comment?.Length > 4000)
        {
            throw new BusinessException("Извините, но комментарий слишком длинный");
        }

        if (model.Place?.Length > 500)
        {
            throw new BusinessException("Извините, но название места слишком длинное");
        }

        if (model.Date == default)
        {
            throw new BusinessException("Извините, дата обязательна");
        }
    }

    private async Task<Data.Entities.Operation> GetByIdInternal(int id, CancellationToken cancellationToken = default)
    {
        var dbCategory = await context.Operations
                             .IsUserEntity(environment.UserId, id)
                             .FirstOrDefaultAsync(cancellationToken)
                         ?? throw new NotFoundException("операция", id);

        return dbCategory;
    }

    private IQueryable<Data.Entities.Operation> FilterOperations(OperationFilter filter)
    {
        var entities = context.Operations
            .IsUserEntity(environment.UserId);

        if (filter.DateFrom.HasValue)
        {
            entities = entities.Where(x => x.Date >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            entities = entities.Where(x => x.Date < filter.DateTo.Value);
        }

        if (filter.CategoryIds is { Count: > 0 })
        {
            entities = entities.Where(x => filter.CategoryIds.Contains(x.CategoryId));
        }

        if (string.IsNullOrEmpty(filter.Comment) == false)
        {
            entities = entities.Where(x => x.Comment != null && x.Comment.Contains(filter.Comment)); // todo сделать регистронезависимый поиск
        }

        if (string.IsNullOrEmpty(filter.Place) == false)
        {
            var placesIds = context.Places
                .Where(x => x.UserId == environment.UserId && x.Name.Contains(filter.Place)) // todo сделать регистронезависимый поиск
                .Select(x => x.Id);

            entities = entities.Where(x => x.PlaceId != null && placesIds.Contains(x.PlaceId.Value));
        }

        return entities;
    }
}
