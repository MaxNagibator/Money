using Money.Business.Mappers;
using Money.Data.Extensions;
using Place = Money.Business.Models.Place;

namespace Money.Business.Services;

public class PlaceService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService)
{
    public Task<List<Place>> GetPlacesAsync(int offset, int count, string? name, CancellationToken cancellationToken)
    {
        IQueryable<Data.Entities.Place> dbPlaces = context.Places
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

        Data.Entities.Place? dbPlace = await context.Places
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Name == place, cancellationToken);

        if (dbPlace == null)
        {
            Data.Entities.DomainUser dbUser = await userService.GetCurrent(cancellationToken);
            int newPlaceId = dbUser.NextPlaceId;
            dbUser.NextPlaceId++;

            dbPlace = new Data.Entities.Place
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

    public async Task<int?> GetPlaceIdAsync(string? place, Data.Entities.OperationBase dbOperation, CancellationToken cancellationToken)
    {
        Data.Entities.Place? dbPlace = await GetPlaceByIdAsync(dbOperation.PlaceId, cancellationToken);
        bool hasAnyOperations = await IsPlaceBusyAsync(dbPlace, dbOperation.Id, cancellationToken);

        if (string.IsNullOrWhiteSpace(place))
        {
            if (dbPlace != null && hasAnyOperations == false)
            {
                dbPlace.IsDeleted = true;
            }

            return null;
        }

        Data.Entities.Place? dbNewPlace = await context.Places
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
                Data.Entities.DomainUser dbUser = await userService.GetCurrent(cancellationToken);
                int newPlaceId = dbUser.NextPlaceId;
                dbUser.NextPlaceId++;

                dbNewPlace = new Data.Entities.Place
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

    private Task<bool> IsPlaceBusyAsync(Data.Entities.Place? place, int? operationId, CancellationToken cancellationToken)
    {
        if (place == null)
        {
            return Task.FromResult(false);
        }

        IQueryable<Data.Entities.Operation> operations = context.Operations
            .IsUserEntity(place.UserId)
            .Where(x => x.PlaceId == place.Id);

        if (operationId != null)
        {
            operations = operations.Where(x => x.Id != operationId.Value);
        }

        return operations.AnyAsync(cancellationToken);
    }

    public async Task CheckRemovePlaceAsync(int? placeId, int? operationId, CancellationToken cancellationToken)
    {
        Data.Entities.Place? dbPlace = await GetPlaceByIdAsync(placeId, cancellationToken);

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

    public async Task CheckRestorePlaceAsync(int? placeId, CancellationToken cancellationToken)
    {
        Data.Entities.Place? dbPlace = await GetPlaceByIdAsync(placeId, cancellationToken);

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
}
