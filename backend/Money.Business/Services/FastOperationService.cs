using Money.Business.Mappers;
using Money.Data.Extensions;

namespace Money.Business.Services;

public class FastOperationService(RequestEnvironment environment, ApplicationDbContext context, CategoryService categoryService)
{
    public async Task<IEnumerable<FastOperation>> GetAsync(CancellationToken cancellationToken)
    {
        IQueryable<DomainFastOperation> dbOperations = context.FastOperations
            .IsUserEntity(environment.UserId);

        List<int> placeIds = await dbOperations
            .Where(x => x.PlaceId != null)
            .Select(x => x.PlaceId!.Value)
            .ToListAsync(cancellationToken);

        List<Place> places = await GetPlacesAsync(placeIds, cancellationToken);

        List<DomainFastOperation> operations = await dbOperations
            .OrderBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        return operations.Select(x => x.Adapt(places)).ToList();
    }

    public async Task<FastOperation> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        DomainFastOperation dbOperation = await GetByIdInternal(id, cancellationToken);

        List<Place>? places = null;

        if (dbOperation.PlaceId != null)
        {
            places = await GetPlacesAsync([dbOperation.PlaceId.Value], cancellationToken);
        }

        return dbOperation.Adapt(places);
    }

    private async Task<DomainFastOperation> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        DomainFastOperation dbOperation = await context.FastOperations
                                         .IsUserEntity(environment.UserId, id)
                                         .FirstOrDefaultAsync(cancellationToken)
                                     ?? throw new NotFoundException("операция", id);

        return dbOperation;
    }

    public async Task<int> CreateAsync(FastOperation operation, CancellationToken cancellationToken)
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

        DomainFastOperation dbOperation = new()
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

    public async Task UpdateAsync(FastOperation operation, CancellationToken cancellationToken)
    {
        DomainFastOperation dbOperation = await context.FastOperations.SingleOrDefaultAsync(environment.UserId, operation.Id, cancellationToken)
                                      ?? throw new NotFoundException("Быстрая операция", operation.Id);

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

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        DomainFastOperation dbOperation = await GetByIdInternal(id, cancellationToken);
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
        DomainFastOperation dbOperation = await context.FastOperations
                                          .IgnoreQueryFilters()
                                          .Where(x => x.IsDeleted)
                                          .SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                      ?? throw new NotFoundException("Быстрая операция", id);

        dbOperation.IsDeleted = false;
        await CheckRestorePlaceAsync(dbOperation.PlaceId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
