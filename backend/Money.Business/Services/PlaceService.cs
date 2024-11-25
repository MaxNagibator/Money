using Money.Business.Mappers;
using Money.Data.Extensions;

namespace Money.Business.Services;

public class PlaceService(RequestEnvironment environment, ApplicationDbContext context)
{
    private Task<List<Place>> GetPlacesAsync(List<int> placeIds, CancellationToken cancellationToken)
    {
        return context.Places
            .Where(x => x.UserId == environment.UserId && placeIds.Contains(x.Id))
            .Select(x => x.Adapt())
            .ToListAsync(cancellationToken);
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

    private async Task CheckRestorePlaceAsync(int? placeId, CancellationToken cancellationToken)
    {
        DomainPlace? dbPlace = await GetPlaceByIdAsync(placeId, cancellationToken);

        if (dbPlace == null)
        {
            return;
        }

        dbPlace.IsDeleted = false;
    }

    private Task<DomainPlace?> GetPlaceByIdAsync(int? placeId, CancellationToken cancellationToken)
    {
        if (placeId != null)
        {
            return context.Places
                .IsUserEntity(environment.UserId, placeId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return Task.FromResult<DomainPlace?>(null);
    }
}
