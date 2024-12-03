using Money.Business.Mappers;
using Money.Data.Extensions;
using Category = Money.Business.Models.Category;
using FastOperation = Money.Business.Models.FastOperation;
using Place = Money.Business.Models.Place;

namespace Money.Business.Services;

public class FastOperationService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService,
    CategoryService categoryService,
    PlaceService placeService)
{
    public async Task<IEnumerable<FastOperation>> GetAsync(CancellationToken cancellationToken)
    {
        IQueryable<Data.Entities.FastOperation> dbOperations = context.FastOperations
            .IsUserEntity(environment.UserId);

        List<int> placeIds = await dbOperations
            .Where(x => x.PlaceId != null)
            .Select(x => x.PlaceId!.Value)
            .ToListAsync(cancellationToken);

        List<Place> places = await placeService.GetPlacesAsync(placeIds, cancellationToken);

        List<Data.Entities.FastOperation> operations = await dbOperations
            .OrderBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        return operations.Select(x => x.Adapt(places)).ToList();
    }

    public async Task<FastOperation> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.FastOperation dbOperation = await GetByIdInternal(id, cancellationToken);

        List<Place>? places = null;

        if (dbOperation.PlaceId != null)
        {
            places = await placeService.GetPlacesAsync([dbOperation.PlaceId.Value], cancellationToken);
        }

        return dbOperation.Adapt(places);
    }

    public async Task<int> CreateAsync(FastOperation operation, CancellationToken cancellationToken)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        DomainUser dbUser = await userService.GetCurrent(cancellationToken);
        Category category = await categoryService.GetByIdAsync(operation.CategoryId, cancellationToken);

        int operationId = dbUser.NextFastOperationId;
        dbUser.NextFastOperationId++;

        int? placeId = await placeService.GetPlaceIdAsync(operation.Place, cancellationToken);

        Data.Entities.FastOperation dbOperation = new()
        {
            Id = operationId,
            Name = operation.Name,
            UserId = environment.UserId.Value,
            CategoryId = category.Id,
            Sum = operation.Sum,
            Comment = operation.Comment,
            PlaceId = placeId,
            Order = operation.Order,
        };

        await context.FastOperations.AddAsync(dbOperation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return operationId;
    }

    public async Task UpdateAsync(FastOperation operation, CancellationToken cancellationToken)
    {
        Data.Entities.FastOperation dbOperation = await context.FastOperations
                                                      .IsUserEntity(environment.UserId, operation.Id)
                                                      .FirstOrDefaultAsync(cancellationToken)
                                                  ?? throw new NotFoundException("Быстрая операция", operation.Id);

        Category category = await categoryService.GetByIdAsync(operation.CategoryId, cancellationToken);
        int? placeId = await placeService.GetPlaceIdAsync(operation.Place, dbOperation, cancellationToken);

        dbOperation.Sum = operation.Sum;
        dbOperation.Comment = operation.Comment;
        dbOperation.CategoryId = category.Id;
        dbOperation.PlaceId = placeId;
        dbOperation.Order = operation.Order;
        dbOperation.Name = operation.Name;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.FastOperation dbOperation = await GetByIdInternal(id, cancellationToken);
        dbOperation.IsDeleted = true;
        await placeService.CheckRemovePlaceAsync(dbOperation.PlaceId, dbOperation.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.FastOperation dbOperation = await context.FastOperations
                                                      .IgnoreQueryFilters()
                                                      .Where(x => x.IsDeleted)
                                                      .IsUserEntity(environment.UserId, id)
                                                      .FirstOrDefaultAsync(cancellationToken)
                                                  ?? throw new NotFoundException("Быстрая операция", id);

        dbOperation.IsDeleted = false;
        await placeService.CheckRestorePlaceAsync(dbOperation.PlaceId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Data.Entities.FastOperation> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        Data.Entities.FastOperation dbOperation = await context.FastOperations
                                                      .IsUserEntity(environment.UserId, id)
                                                      .FirstOrDefaultAsync(cancellationToken)
                                                  ?? throw new NotFoundException("операция", id);

        return dbOperation;
    }
}
