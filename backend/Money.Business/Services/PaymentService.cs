using Money.Business.Mappers;
using Money.Data.Extensions;

namespace Money.Business.Services;

public class PaymentService(RequestEnvironment environment, ApplicationDbContext context, CategoryService categoryService)
{
    public async Task<ICollection<Operation>> GetAsync(PaymentFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<DomainPayment> dbPayments = FilterPayments(filter);

        List<int> placeIds = await dbPayments
            .Where(x => x.PlaceId != null)
            .Select(x => x.PlaceId!.Value)
            .ToListAsync(cancellationToken);

        List<DomainPlace> dbPlaces = await GetPlacesAsync(placeIds, cancellationToken);

        List<DomainPayment> dbPaymentList = await dbPayments
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        return dbPaymentList.Select(x => x.Adapt(dbPlaces)).ToList();
    }

    public async Task<Operation> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        DomainPayment dbPayment = await GetByIdInternal(id, cancellationToken);

        List<DomainPlace> dbPlaces = dbPayment.PlaceId != null
            ? await GetPlacesAsync([dbPayment.PlaceId.Value], cancellationToken)
            : [];

        Operation payment = dbPayment.Adapt(dbPlaces);
        return payment;
    }

    private async Task<DomainPayment> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        DomainPayment dbCategory = await context.Payments.SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                   ?? throw new NotFoundException("платеж", id);

        return dbCategory;
    }

    private IQueryable<DomainPayment> FilterPayments(PaymentFilter filter)
    {
        IQueryable<DomainPayment> dbPayments = context.Payments.IsUserEntity(environment.UserId)
            .Where(x => x.TaskId == null);

        if (filter.DateFrom.HasValue)
        {
            dbPayments = dbPayments.Where(x => x.Date >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            dbPayments = dbPayments.Where(x => x.Date < filter.DateTo.Value);
        }

        if (filter.CategoryIds is { Count: > 0 })
        {
            dbPayments = dbPayments.Where(x => filter.CategoryIds.Contains(x.CategoryId));
        }

        if (string.IsNullOrEmpty(filter.Comment) == false)
        {
            dbPayments = dbPayments.Where(x => x.Comment != null && x.Comment.Contains(filter.Comment)); // todo сделать регистронезависимый поиск
        }

        if (string.IsNullOrEmpty(filter.Place) == false)
        {
            IQueryable<int> placesIds = context.Places
                .Where(x => x.UserId == environment.UserId && x.Name.Contains(filter.Place)) // todo сделать регистронезависимый поиск
                .Select(x => x.Id);

            dbPayments = dbPayments.Where(x => x.PlaceId != null && placesIds.Contains(x.PlaceId.Value));
        }

        return dbPayments;
    }

    private async Task<List<DomainPlace>> GetPlacesAsync(List<int> placeIds, CancellationToken cancellationToken)
    {
        return await context.Places
            .Where(x => x.UserId == environment.UserId && placeIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CreateAsync(Operation payment, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        DomainUser dbUser = await context.DomainUsers.SingleOrDefaultAsync(x => x.Id == environment.UserId, cancellationToken)
                            ?? throw new BusinessException("Извините, но пользователь не найден.");

        Category category = await categoryService.GetByIdAsync(payment.CategoryId, cancellationToken);

        int paymentId = dbUser.NextPaymentId;
        dbUser.NextPaymentId++;

        int? placeId = await GetPlaceId(dbUser, payment.Place, cancellationToken);

        DomainPayment dbPayment = new()
        {
            Id = paymentId,
            UserId = environment.UserId.Value,
            CategoryId = category.Id,
            Sum = payment.Sum,
            Comment = payment.Comment,
            Date = payment.Date,
            PlaceId = placeId,
            CreatedTaskId = payment.CreatedTaskId,
        };

        await context.Payments.AddAsync(dbPayment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return paymentId;
    }

    private async Task<int?> GetPlaceId(DomainUser dbUser, string? place, CancellationToken cancellationToken = default)
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

    public async Task UpdateAsync(Operation payment, CancellationToken cancellationToken)
    {
        DomainPayment dbPayment = await context.Payments.SingleOrDefaultAsync(environment.UserId, payment.Id, cancellationToken)
                                  ?? throw new NotFoundException("платеж", payment.Id);

        Category category = await categoryService.GetByIdAsync(payment.CategoryId, cancellationToken);
        DomainUser dbUser = await context.DomainUsers.SingleAsync(x => x.Id == environment.UserId, cancellationToken);
        int? placeId = await GetPlaceId(dbUser, payment.Place, dbPayment, cancellationToken);

        dbPayment.Sum = payment.Sum;
        dbPayment.Comment = payment.Comment;
        dbPayment.Date = payment.Date;
        dbPayment.CategoryId = category.Id;
        dbPayment.PlaceId = placeId;

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<int?> GetPlaceId(DomainUser dbUser, string? place, DomainPayment dbPayment, CancellationToken cancellationToken = default)
    {
        DomainPlace? dbPlace = await GetPlaceById(dbPayment.PlaceId, cancellationToken);
        bool hasAnyPayments = await IsPlaceBusy(dbPlace, dbPayment.Id, cancellationToken);

        if (string.IsNullOrWhiteSpace(place))
        {
            if (dbPlace != null && hasAnyPayments == false)
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
            if (dbPlace != null && hasAnyPayments == false && dbPlace.Id != dbNewPlace.Id)
            {
                dbPlace.IsDeleted = true;
            }
        }
        else
        {
            if (dbPlace != null && hasAnyPayments == false)
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

    private Task<bool> IsPlaceBusy(DomainPlace? place, int? paymentId, CancellationToken cancellationToken = default)
    {
        if (place == null)
        {
            return Task.FromResult(false);
        }

        IQueryable<DomainPayment> payments = context.Payments
            .IsUserEntity(place.UserId)
            .Where(x => x.PlaceId == place.Id);

        if (paymentId != null)
        {
            payments = payments.Where(x => x.Id != paymentId.Value);
        }

        return payments.AnyAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        DomainPayment dbPayment = await GetByIdInternal(id, cancellationToken);
        dbPayment.IsDeleted = true;
        await CheckRemovePlace(dbPayment.PlaceId, dbPayment.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckRemovePlace(int? placeId, int? paymentId, CancellationToken cancellationToken = default)
    {
        DomainPlace? dbPlace = await GetPlaceById(placeId, cancellationToken);

        if (dbPlace == null)
        {
            return;
        }

        bool hasAnyPayments = await IsPlaceBusy(dbPlace, paymentId, cancellationToken);

        if (hasAnyPayments == false)
        {
            dbPlace.IsDeleted = true;
        }
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        DomainPayment dbPayment = await context.Payments
                                      .IgnoreQueryFilters()
                                      .Where(x => x.IsDeleted)
                                      .SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                  ?? throw new NotFoundException("платеж", id);

        dbPayment.IsDeleted = false;
        await CheckRestorePlace(dbPayment.PlaceId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckRestorePlace(int? placeId, CancellationToken cancellationToken = default)
    {
        DomainPlace? dbPlace = await GetPlaceById(placeId, cancellationToken);

        if (dbPlace == null)
        {
            return;
        }

        dbPlace.IsDeleted = false;
    }

    private async Task<DomainPlace?> GetPlaceById(int? placeId, CancellationToken cancellationToken = default)
    {
        return placeId != null
            ? await context.Places.SingleOrDefaultAsync(environment.UserId, placeId, cancellationToken)
            : null;
    }

    public async Task<ICollection<Place>> GetPlaces(int offset, int count, string name, CancellationToken cancellationToken = default)
    {
        var dbPlaces = context.Places.Where(x => x.UserId == environment.UserId && x.IsDeleted == false).AsQueryable();
        if (!string.IsNullOrEmpty(name))
        {
            dbPlaces = dbPlaces.Where(x => x.Name.Contains(name)); // todo сделать регистронезависимый поиск
        }

        var placeList = await dbPlaces
            .OrderByDescending(x => x.Name.StartsWith(name))
            .ThenByDescending(x => x.LastUsedDate)
            .Skip(offset)
            .Take(count)
            .ToListAsync(cancellationToken);

        return placeList.Select(x => new Place { Id = x.Id, Name = x.Name }).ToList();
    }
}
