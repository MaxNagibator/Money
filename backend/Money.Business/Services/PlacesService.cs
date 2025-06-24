using Money.Data.Extensions;
using OperationBase = Money.Data.Entities.Base.OperationBase;
using Place = Money.Business.Models.Place;

namespace Money.Business.Services;

public class PlacesService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UsersService usersService)
{
    public Task<List<Place>> GetPlacesAsync(int offset, int count, string? name, CancellationToken cancellationToken = default)
    {
        var entities = context.Places
            .IsUserEntity(environment.UserId)
            .Where(x => x.IsDeleted == false);

        if (string.IsNullOrWhiteSpace(name) == false)
        {
            entities = entities.Where(x => x.Name.Contains(name));
        }

        return entities
            .OrderByDescending(x => string.IsNullOrWhiteSpace(name) == false && x.Name.StartsWith(name))
            .ThenByDescending(x => x.LastUsedDate)
            .Skip(offset)
            .Take(count)
            .Select(x => GetBusinessModel(x))
            .ToListAsync(cancellationToken);
    }

    public Task<List<Place>> GetPlacesAsync(List<int> placeIds, CancellationToken cancellationToken = default)
    {
        return context.Places
            .IsUserEntity(environment.UserId)
            .Where(x => placeIds.Contains(x.Id))
            .Select(x => GetBusinessModel(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<int?> GetPlaceIdAsync(string? place, CancellationToken cancellationToken = default)
    {
        place = place?.Trim(' ');

        if (string.IsNullOrWhiteSpace(place))
        {
            return null;
        }

        var entity = await context.Places
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Name == place, cancellationToken);

        if (entity == null)
        {
            var newPlaceId = await usersService.GetNextPlaceIdAsync(cancellationToken);

            entity = new()
            {
                UserId = await usersService.GetIdAsync(cancellationToken),
                Id = newPlaceId,
                Name = place,
            };

            await context.Places.AddAsync(entity, cancellationToken);
        }

        entity.LastUsedDate = DateTime.Now;
        entity.Name = place;
        entity.IsDeleted = false;
        return entity.Id;
    }

    public async Task<int?> GetPlaceIdAsync(string? place, OperationBase dbOperation, CancellationToken cancellationToken = default)
    {
        var entity = await GetPlaceByIdAsync(dbOperation.PlaceId, cancellationToken);
        var hasAnyOperations = await IsPlaceBusyAsync(entity, dbOperation.Id, cancellationToken);

        if (string.IsNullOrWhiteSpace(place))
        {
            if (entity != null && hasAnyOperations == false)
            {
                entity.IsDeleted = true;
            }

            return null;
        }

        var dbNewPlace = await context.Places
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Name == place, cancellationToken);

        if (dbNewPlace != null)
        {
            if (entity != null && hasAnyOperations == false && entity.Id != dbNewPlace.Id)
            {
                entity.IsDeleted = true;
            }
        }
        else
        {
            if (entity != null && hasAnyOperations == false)
            {
                dbNewPlace = entity;
            }
            else
            {
                var newPlaceId = await usersService.GetNextPlaceIdAsync(cancellationToken);

                dbNewPlace = new()
                {
                    Name = "",
                    UserId = await usersService.GetIdAsync(cancellationToken),
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

    public async Task CheckRemovePlaceAsync(int? placeId, int? operationId, CancellationToken cancellationToken = default)
    {
        var entity = await GetPlaceByIdAsync(placeId, cancellationToken);

        if (entity == null)
        {
            return;
        }

        var hasAnyOperations = await IsPlaceBusyAsync(entity, operationId, cancellationToken);

        if (hasAnyOperations == false)
        {
            entity.IsDeleted = true;
        }
    }

    public async Task CheckRestorePlaceAsync(int? placeId, CancellationToken cancellationToken = default)
    {
        var entity = await GetPlaceByIdAsync(placeId, cancellationToken);

        if (entity == null)
        {
            return;
        }

        entity.IsDeleted = false;
    }

    private static Place GetBusinessModel(Data.Entities.Place model)
    {
        return new()
        {
            Id = model.Id,
            Name = model.Name,
        };
    }

    private Task<Data.Entities.Place?> GetPlaceByIdAsync(int? placeId, CancellationToken cancellationToken = default)
    {
        if (placeId != null)
        {
            return context.Places
                .IsUserEntity(environment.UserId, placeId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return Task.FromResult<Data.Entities.Place?>(null);
    }

    private Task<bool> IsPlaceBusyAsync(Data.Entities.Place? model, int? operationId, CancellationToken cancellationToken = default)
    {
        if (model == null)
        {
            return Task.FromResult(false);
        }

        var operations = context.Operations
            .IsUserEntity(model.UserId)
            .Where(x => x.PlaceId == model.Id);

        if (operationId != null)
        {
            operations = operations.Where(x => x.Id != operationId.Value);
        }

        return operations.AnyAsync(cancellationToken);
    }
}
