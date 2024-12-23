using Money.Business.Mappers;
using Money.Data.Extensions;
namespace Money.Business.Services;

// TODO: PlaceService выкинуть приватные методы и заюзать (+ проверить что конструктор работает с одним и тем жи контекстом)
public class OperationService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService,
    CategoryService categoryService,
    PlaceService placeService)
{
    public async Task<IEnumerable<Operation>> GetAsync(OperationFilter filter, CancellationToken cancellationToken)
    {
        IQueryable<Data.Entities.Operation> filteredOperations = FilterOperations(filter);

        List<int> placeIds = await filteredOperations
            .Where(x => x.PlaceId != null)
            .Select(x => x.PlaceId!.Value)
            .ToListAsync(cancellationToken);

        List<Place> places = await placeService.GetPlacesAsync(placeIds, cancellationToken);

        List<Data.Entities.Operation> operations = await filteredOperations
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        return operations.Select(x => x.Adapt(places)).ToList();
    }

    public async Task<Operation> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.Operation dbOperation = await GetByIdInternal(id, cancellationToken);

        List<Place>? places = null;

        if (dbOperation.PlaceId != null)
        {
            places = await placeService.GetPlacesAsync([dbOperation.PlaceId.Value], cancellationToken);
        }

        return dbOperation.Adapt(places);
    }

    public async Task<int> CreateAsync(Operation operation, CancellationToken cancellationToken)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        Data.Entities.DomainUser dbUser = await userService.GetCurrent(cancellationToken);
        Category category = await categoryService.GetByIdAsync(operation.CategoryId, cancellationToken);

        int operationId = dbUser.NextOperationId;
        dbUser.NextOperationId++;

        int? placeId = operation.PlaceId ?? await placeService.GetPlaceIdAsync(operation.Place, cancellationToken);

        Data.Entities.Operation dbOperation = new()
        {
            Id = operationId,
            UserId = environment.UserId.Value,
            CategoryId = category.Id,
            Sum = operation.Sum,
            Comment = operation.Comment,
            Date = operation.Date,
            PlaceId = placeId,
            CreatedTaskId = operation.CreatedTaskId,
        };

        await context.Operations.AddAsync(dbOperation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return operationId;
    }

    public async Task UpdateAsync(Operation operation, CancellationToken cancellationToken)
    {
        Data.Entities.Operation dbOperation = await context.Operations.SingleOrDefaultAsync(environment.UserId, operation.Id, cancellationToken)
                                              ?? throw new NotFoundException("операция", operation.Id);

        Category category = await categoryService.GetByIdAsync(operation.CategoryId, cancellationToken);
        int? placeId = await placeService.GetPlaceIdAsync(operation.Place, dbOperation, cancellationToken);

        dbOperation.Sum = operation.Sum;
        dbOperation.Comment = operation.Comment;
        dbOperation.Date = operation.Date;
        dbOperation.CategoryId = category.Id;
        dbOperation.PlaceId = placeId;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.Operation dbOperation = await GetByIdInternal(id, cancellationToken);
        dbOperation.IsDeleted = true;
        await placeService.CheckRemovePlaceAsync(dbOperation.PlaceId, dbOperation.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.Operation dbOperation = await context.Operations
                                                  .IgnoreQueryFilters()
                                                  .Where(x => x.IsDeleted)
                                                  .SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                              ?? throw new NotFoundException("операция", id);

        dbOperation.IsDeleted = false;
        await placeService.CheckRestorePlaceAsync(dbOperation.PlaceId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Operation>> UpdateBatchAsync(List<int> operationIds, int categoryId, CancellationToken cancellationToken)
    {
        Category category = await categoryService.GetByIdAsync(categoryId, cancellationToken);

        List<Data.Entities.Operation> dbOperations = await context.Operations
            .IsUserEntity(environment.UserId)
            .Where(x => operationIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (dbOperations.Count != operationIds.Count)
        {
            throw new BusinessException("Одна или несколько операций не найдены");
        }

        foreach (Data.Entities.Operation? operation in dbOperations)
        {
            operation.CategoryId = category.Id;
        }

        await context.SaveChangesAsync(cancellationToken);

        return dbOperations.Select(x => x.Adapt()).ToList();
    }

    private async Task<Data.Entities.Operation> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        Data.Entities.Operation dbCategory = await context.Operations
                                                 .IsUserEntity(environment.UserId, id)
                                                 .FirstOrDefaultAsync(cancellationToken)
                                             ?? throw new NotFoundException("операция", id);

        return dbCategory;
    }

    private IQueryable<Data.Entities.Operation> FilterOperations(OperationFilter filter)
    {
        IQueryable<Data.Entities.Operation> dbOperations = context.Operations
            .IsUserEntity(environment.UserId);

        if (filter.DateFrom.HasValue)
        {
            dbOperations = dbOperations.Where(x => x.Date >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            dbOperations = dbOperations.Where(x => x.Date < filter.DateTo.Value);
        }

        if (filter.CategoryIds is { Count: > 0 })
        {
            dbOperations = dbOperations.Where(x => filter.CategoryIds.Contains(x.CategoryId));
        }

        if (string.IsNullOrEmpty(filter.Comment) == false)
        {
            dbOperations = dbOperations.Where(x => x.Comment != null && x.Comment.Contains(filter.Comment)); // todo сделать регистронезависимый поиск
        }

        if (string.IsNullOrEmpty(filter.Place) == false)
        {
            IQueryable<int> placesIds = context.Places
                .Where(x => x.UserId == environment.UserId && x.Name.Contains(filter.Place)) // todo сделать регистронезависимый поиск
                .Select(x => x.Id);

            dbOperations = dbOperations.Where(x => x.PlaceId != null && placesIds.Contains(x.PlaceId.Value));
        }

        return dbOperations;
    }
}
