using Money.Business.Mappers;
using Money.Data.Extensions;

namespace Money.Business.Services;

public class OperationService(RequestEnvironment environment, ApplicationDbContext context, CategoryService categoryService)
{
    public async Task<IEnumerable<Operation>> GetAsync(OperationFilter filter, CancellationToken cancellationToken)
    {
        IQueryable<DomainOperation> dbOperations = FilterOperations(filter);

        List<int> placeIds = await dbOperations
            .Where(x => x.PlaceId != null)
            .Select(x => x.PlaceId!.Value)
            .ToListAsync(cancellationToken);

        List<DomainPlace> dbPlaces = await GetPlacesAsync(placeIds, cancellationToken);

        List<DomainOperation> dbOperationList = await dbOperations
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        return dbOperationList.Select(x => x.Adapt(dbPlaces)).ToList();
    }

    public async Task<Operation> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        DomainOperation dbOperation = await GetByIdInternal(id, cancellationToken);

        List<DomainPlace> dbPlaces = dbOperation.PlaceId != null
            ? await GetPlacesAsync([dbOperation.PlaceId.Value], cancellationToken)
            : [];

        Operation operation = dbOperation.Adapt(dbPlaces);
        return operation;
    }

    private async Task<DomainOperation> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        DomainOperation dbCategory = await context.Operations.SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                     ?? throw new NotFoundException("операция", id);

        return dbCategory;
    }

    private IQueryable<DomainOperation> FilterOperations(OperationFilter filter)
    {
        IQueryable<DomainOperation> dbOperations = context.Operations
            .IsUserEntity(environment.UserId)
            .Where(x => x.TaskId == null);

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

    private async Task<List<DomainPlace>> GetPlacesAsync(List<int> placeIds, CancellationToken cancellationToken)
    {
        return await context.Places
            .Where(x => x.UserId == environment.UserId && placeIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CreateAsync(Operation operation, CancellationToken cancellationToken)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        DomainUser dbUser = await context.DomainUsers.SingleOrDefaultAsync(x => x.Id == environment.UserId, cancellationToken)
                            ?? throw new BusinessException("Извините, но пользователь не найден.");

        Category category = await categoryService.GetByIdAsync(operation.CategoryId, cancellationToken);

        int operationId = dbUser.NextOperationId;
        dbUser.NextOperationId++;

        int? placeId = await GetPlaceIdAsync(dbUser, operation.Place, cancellationToken);

        DomainOperation dbOperation = new()
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

    private async Task<int?> GetPlaceIdAsync(DomainUser dbUser, string? place, CancellationToken cancellationToken)
    {
        place = place?.Trim(' ');

        if (string.IsNullOrWhiteSpace(place))
        {
            return null;
        }

        DomainPlace? dbPlace = await context.Places
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Name == place, cancellationToken);

        if (dbPlace == null)
        {
            int newPlaceId = dbUser.NextPlaceId;
            dbUser.NextPlaceId++;

            dbPlace = new DomainPlace
            {
                UserId = dbUser.Id,
                Id = newPlaceId,
                Name = place,
            };

            await context.Places.AddAsync(dbPlace, cancellationToken);
        }

        dbPlace.LastUsedDate = DateTime.Now;
        dbPlace.Name = place;
        dbPlace.IsDeleted = false;
        return dbPlace.Id;
    }

    public async Task UpdateAsync(Operation operation, CancellationToken cancellationToken)
    {
        DomainOperation dbOperation = await context.Operations.SingleOrDefaultAsync(environment.UserId, operation.Id, cancellationToken)
                                      ?? throw new NotFoundException("операция", operation.Id);

        Category category = await categoryService.GetByIdAsync(operation.CategoryId, cancellationToken);
        DomainUser dbUser = await context.DomainUsers.SingleAsync(x => x.Id == environment.UserId, cancellationToken);
        int? placeId = await GetPlaceIdAsync(dbUser, operation.Place, dbOperation, cancellationToken);

        dbOperation.Sum = operation.Sum;
        dbOperation.Comment = operation.Comment;
        dbOperation.Date = operation.Date;
        dbOperation.CategoryId = category.Id;
        dbOperation.PlaceId = placeId;

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<int?> GetPlaceIdAsync(DomainUser dbUser, string? place, DomainOperation dbOperation, CancellationToken cancellationToken)
    {
        DomainPlace? dbPlace = await GetPlaceByIdAsync(dbOperation.PlaceId, cancellationToken);
        bool hasAnyOperations = await IsPlaceBusyAsync(dbPlace, dbOperation.Id, cancellationToken);

        if (string.IsNullOrWhiteSpace(place))
        {
            if (dbPlace != null && hasAnyOperations == false)
            {
                dbPlace.IsDeleted = true;
            }

            return null;
        }

        DomainPlace? dbNewPlace = await context.Places
            .IsUserEntity(dbUser.Id)
            .SingleOrDefaultAsync(x => x.Name == place, cancellationToken);

        if (dbNewPlace != null)
        {
            if (dbPlace != null && hasAnyOperations == false && dbPlace.Id != dbNewPlace.Id)
            {
                dbPlace.IsDeleted = true;
            }
        }
        else
        {
            if (dbPlace != null && hasAnyOperations == false)
            {
                dbNewPlace = dbPlace;
            }
            else
            {
                int newPlaceId = dbUser.NextPlaceId;
                dbUser.NextPlaceId++;

                dbNewPlace = new DomainPlace
                {
                    Name = "",
                    UserId = dbUser.Id,
                    Id = newPlaceId,
                };

                await context.Places.AddAsync(dbNewPlace, cancellationToken);
            }
        }

        dbNewPlace.LastUsedDate = DateTime.Now;
        dbNewPlace.IsDeleted = false;
        dbNewPlace.Name = place;

        return dbNewPlace.Id;
    }

    private Task<bool> IsPlaceBusyAsync(DomainPlace? place, int? operationId, CancellationToken cancellationToken)
    {
        if (place == null)
        {
            return Task.FromResult(false);
        }

        IQueryable<DomainOperation> operations = context.Operations
            .IsUserEntity(place.UserId)
            .Where(x => x.PlaceId == place.Id);

        if (operationId != null)
        {
            operations = operations.Where(x => x.Id != operationId.Value);
        }

        return operations.AnyAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        DomainOperation dbOperation = await GetByIdInternal(id, cancellationToken);
        dbOperation.IsDeleted = true;
        await CheckRemovePlaceAsync(dbOperation.PlaceId, dbOperation.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckRemovePlaceAsync(int? placeId, int? operationId, CancellationToken cancellationToken)
    {
        DomainPlace? dbPlace = await GetPlaceByIdAsync(placeId, cancellationToken);

        if (dbPlace == null)
        {
            return;
        }

        bool hasAnyOperations = await IsPlaceBusyAsync(dbPlace, operationId, cancellationToken);

        if (hasAnyOperations == false)
        {
            dbPlace.IsDeleted = true;
        }
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken)
    {
        DomainOperation dbOperation = await context.Operations
                                          .IgnoreQueryFilters()
                                          .Where(x => x.IsDeleted)
                                          .SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                      ?? throw new NotFoundException("операция", id);

        dbOperation.IsDeleted = false;
        await CheckRestorePlaceAsync(dbOperation.PlaceId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckRestorePlaceAsync(int? placeId, CancellationToken cancellationToken)
    {
        DomainPlace? dbPlace = await GetPlaceByIdAsync(placeId, cancellationToken);

        if (dbPlace == null)
        {
            return;
        }

        dbPlace.IsDeleted = false;
    }

    private async Task<DomainPlace?> GetPlaceByIdAsync(int? placeId, CancellationToken cancellationToken)
    {
        return placeId != null
            ? await context.Places.SingleOrDefaultAsync(environment.UserId, placeId, cancellationToken)
            : null;
    }

    public async Task<IEnumerable<Operation>> UpdateBatchAsync(List<int> operationIds, int categoryId, CancellationToken cancellationToken)
    {
        Category category = await categoryService.GetByIdAsync(categoryId, cancellationToken);

        List<DomainOperation> dbOperations = await context.Operations
            .IsUserEntity(environment.UserId)
            .Where(x => operationIds.Contains(x.Id) && x.TaskId == null)
            .ToListAsync(cancellationToken);

        if (dbOperations.Count != operationIds.Count)
        {
            throw new BusinessException("Одна или несколько операций не найдены");
        }

        foreach (DomainOperation? operation in dbOperations)
        {
            operation.CategoryId = category.Id;
        }

        await context.SaveChangesAsync(cancellationToken);

        return dbOperations.Select(x => x.Adapt()).ToList();
    }

    public async Task<IEnumerable<Place>> GetPlacesAsync(int offset, int count, string? name, CancellationToken cancellationToken)
    {
        IQueryable<DomainPlace> dbPlaces = context.Places
            .IsUserEntity(environment.UserId)
            .Where(x => x.IsDeleted == false);

        if (string.IsNullOrWhiteSpace(name) == false)
        {
            dbPlaces = dbPlaces.Where(x => x.Name.Contains(name)); // todo сделать регистронезависимый поиск
        }

        dbPlaces = dbPlaces
            .OrderByDescending(x => string.IsNullOrWhiteSpace(name) == false && x.Name.StartsWith(name))
            .ThenByDescending(x => x.LastUsedDate)
            .Skip(offset)
            .Take(count);

        List<DomainPlace> places = await dbPlaces.ToListAsync(cancellationToken);

        return places
            .Select(x => new Place
            {
                Id = x.Id,
                Name = x.Name,
            })
            .ToList();
    }
}
