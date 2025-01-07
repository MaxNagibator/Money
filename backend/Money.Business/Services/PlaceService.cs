using Money.Business.Mappers;
using Money.Data.Extensions;
using OperationBase = Money.Data.Entities.Base.OperationBase;
using Place = Money.Business.Models.Place;

namespace Money.Business.Services;

public class PlaceService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService)
{
    public Task<List<Place>> GetPlacesAsync(int offset, int count, string? name, CancellationToken cancellationToken)
    {
        var dbPlaces = context.Places
            .IsUserEntity(environment.UserId)
            .Where(x => x.IsDeleted == false);

        if (string.IsNullOrWhiteSpace(name) == false)
        {
            dbPlaces = dbPlaces.Where(x => x.Name.Contains(name)); // todo сделать регистронезависимый поиск
        }

        return dbPlaces
            .OrderByDescending(x => string.IsNullOrWhiteSpace(name) == false && x.Name.StartsWith(name))
            .ThenByDescending(x => x.LastUsedDate)
            .Skip(offset)
            .Take(count)
            .Select(x => x.Adapt())
            .ToListAsync(cancellationToken);
    }

    public Task<List<Place>> GetPlacesAsync(List<int> placeIds, CancellationToken cancellationToken)
    {
        return context.Places
            .IsUserEntity(environment.UserId)
            .Where(x => placeIds.Contains(x.Id))
            .Select(x => x.Adapt())
            .ToListAsync(cancellationToken);
    }

    public async Task<int?> GetPlaceIdAsync(string? place, CancellationToken cancellationToken)
    {
        place = place?.Trim(' ');

        if (string.IsNullOrWhiteSpace(place))
        {
            return null;
        }

        var dbPlace = await context.Places
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Name == place, cancellationToken);

        if (dbPlace == null)
        {
            var newPlaceId = await userService.GetNextPlaceIdAsync(cancellationToken);

            dbPlace = new()
            {
                UserId = await userService.GetIdAsync(cancellationToken),
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

    public async Task<int?> GetPlaceIdAsync(string? place, OperationBase dbOperation, CancellationToken cancellationToken)
    {
        var dbPlace = await GetPlaceByIdAsync(dbOperation.PlaceId, cancellationToken);
        var hasAnyOperations = await IsPlaceBusyAsync(dbPlace, dbOperation.Id, cancellationToken);

        if (string.IsNullOrWhiteSpace(place))
        {
            if (dbPlace != null && hasAnyOperations == false)
            {
                dbPlace.IsDeleted = true;
            }

            return null;
        }

        var dbNewPlace = await context.Places
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Name == place, cancellationToken);

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
                var newPlaceId = await userService.GetNextPlaceIdAsync(cancellationToken);

                dbNewPlace = new()
                {
                    Name = "",
                    UserId = await userService.GetIdAsync(cancellationToken),
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

    public async Task CheckRemovePlaceAsync(int? placeId, int? operationId, CancellationToken cancellationToken)
    {
        var dbPlace = await GetPlaceByIdAsync(placeId, cancellationToken);

        if (dbPlace == null)
        {
            return;
        }

        var hasAnyOperations = await IsPlaceBusyAsync(dbPlace, operationId, cancellationToken);

        if (hasAnyOperations == false)
        {
            dbPlace.IsDeleted = true;
        }
    }

    public async Task CheckRestorePlaceAsync(int? placeId, CancellationToken cancellationToken)
    {
        var dbPlace = await GetPlaceByIdAsync(placeId, cancellationToken);

        if (dbPlace == null)
        {
            return;
        }

        dbPlace.IsDeleted = false;
    }

    private Task<Data.Entities.Place?> GetPlaceByIdAsync(int? placeId, CancellationToken cancellationToken)
    {
        if (placeId != null)
        {
            return context.Places
                .IsUserEntity(environment.UserId, placeId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return Task.FromResult<Data.Entities.Place?>(null);
    }

    private Task<bool> IsPlaceBusyAsync(Data.Entities.Place? place, int? operationId, CancellationToken cancellationToken)
    {
        if (place == null)
        {
            return Task.FromResult(false);
        }

        var operations = context.Operations
            .IsUserEntity(place.UserId)
            .Where(x => x.PlaceId == place.Id);

        if (operationId != null)
        {
            operations = operations.Where(x => x.Id != operationId.Value);
        }

        return operations.AnyAsync(cancellationToken);
    }
}
