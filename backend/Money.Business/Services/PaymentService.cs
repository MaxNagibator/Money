using Microsoft.EntityFrameworkCore;
using Money.Business.Enums;
using Money.Business.Models;
using Money.Common.Exceptions;
using Money.Data;
using Money.Data.Extensions;

namespace Money.Business.Services;

public class PaymentService(RequestEnvironment environment, ApplicationDbContext context, CategoryService categoryService)
{
    public async Task<ICollection<Payment>> GetAsync(PaymentFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<Data.Entities.Payment> dbPayments = FilterPayments(filter);

        List<int> placeIds = await dbPayments
            .Where(x => x.PlaceId != null)
            .Select(x => x.PlaceId!.Value)
            .ToListAsync(cancellationToken);

        List<Data.Entities.Place> dbPlaces = await GetPlacesAsync(placeIds, cancellationToken);

        List<Data.Entities.Payment> dbPaymentList = await dbPayments
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        return dbPaymentList.Select(dbPayment => MapTo(dbPayment, dbPlaces)).ToList();
    }

    public async Task<Payment> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.Payment dbPayment = await GetByIdInternal(id, cancellationToken);
        List<Data.Entities.Place> dbPlaces = (dbPayment.PlaceId == null ?
            null :
            await GetPlacesAsync(new List<int> { dbPayment.PlaceId.Value }, cancellationToken))
            ?? new List<Data.Entities.Place>();
        Payment payment = MapTo(dbPayment, dbPlaces);
        return payment;
    }

    private async Task<Data.Entities.Payment> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        Data.Entities.Payment dbCategory = await context.Payments.SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                            ?? throw new NotFoundException($"Извините, но платеж с ID {id} не найден. Пожалуйста, проверьте правильность введенного ID.");

        return dbCategory;
    }

    private IQueryable<Data.Entities.Payment> FilterPayments(PaymentFilter filter)
    {
        IQueryable<Data.Entities.Payment> dbPayments = context.Payments.IsUserEntity(environment.UserId)
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
            dbPayments = dbPayments.Where(x => x.Comment != null && x.Comment.Contains(filter.Comment));
        }

        if (string.IsNullOrEmpty(filter.Place) == false)
        {
            IQueryable<int> placesIds = context.Places
                .Where(x => x.UserId == environment.UserId && x.Name.Contains(filter.Place))
                .Select(x => x.Id);

            dbPayments = dbPayments.Where(x => x.PlaceId != null && placesIds.Contains(x.PlaceId.Value));
        }

        return dbPayments;
    }

    private async Task<List<Data.Entities.Place>> GetPlacesAsync(List<int> placeIds, CancellationToken cancellationToken)
    {
        return await context.Places
            .Where(x => x.UserId == environment.UserId && placeIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    private Payment MapTo(Data.Entities.Payment dbPayment, List<Data.Entities.Place> dbPlaces)
    {
        return new Payment
        {
            CategoryId = dbPayment.CategoryId,
            Sum = dbPayment.Sum,
            Comment = dbPayment.Comment,
            Place = dbPayment.PlaceId.HasValue
                ? dbPlaces.FirstOrDefault(x => x.Id == dbPayment.PlaceId)?.Name
                : null,
            Id = dbPayment.Id,
            Date = dbPayment.Date,
            CreatedTaskId = dbPayment.CreatedTaskId,
        };
    }

    public async Task<int> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        Category category = await categoryService.GetByIdAsync(payment.CategoryId, cancellationToken);

        Data.Entities.DomainUser dbUser = await context.DomainUsers.SingleAsync(x => x.Id == environment.UserId, cancellationToken);

        int paymentId = dbUser.NextPaymentId;
        dbUser.NextPaymentId++;

        int? placeId = GetPlaceId(dbUser, payment.Place);
        Data.Entities.Payment dbPayment = new()
        {
            Id = paymentId,
            UserId = environment.UserId.Value,
            CategoryId = payment.CategoryId,
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

    public int? GetPlaceId(Data.Entities.DomainUser dbUser, string? place)
    {
        place = place?.Trim(' ');
        if (string.IsNullOrEmpty(place))
        {
            return null;
        }

        var dbPlace = context.Places.FirstOrDefault(x => x.UserId == environment.UserId && x.Name == place);
        if (dbPlace == null)
        {
            var newPlaceId = dbUser.NextPlaceId;
            dbUser.NextPlaceId++;

            dbPlace = new Data.Entities.Place()
            {
                UserId = dbUser.Id,
                Id = newPlaceId,
                Name = place,
            };
            context.Places.Add(dbPlace);
        }

        dbPlace.LastUsedDate = DateTime.Now;
        dbPlace.Name = place;
        return dbPlace.Id;
    }


    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken)
    {
        Data.Entities.Payment dbPayment = await context.Payments.SingleOrDefaultAsync(environment.UserId, payment.Id, cancellationToken)
                                            ?? throw new NotFoundException($"Извините, но платеж с ID {payment.Id} не найден. Пожалуйста, проверьте правильность введенного ID.");

        Category category = await categoryService.GetByIdAsync(payment.CategoryId, cancellationToken);
        Data.Entities.DomainUser dbUser = await context.DomainUsers.SingleAsync(x => x.Id == environment.UserId, cancellationToken);
        int? placeId = GetPlaceId(dbUser, payment.Place, dbPayment);

        dbPayment.Sum = payment.Sum;
        dbPayment.Comment = payment.Comment;
        dbPayment.Date = payment.Date;
        dbPayment.CategoryId = category.Id;
        dbPayment.PlaceId = placeId;

        await context.SaveChangesAsync(cancellationToken);
    }

    public int? GetPlaceId(Data.Entities.DomainUser dbUser, string? place, Data.Entities.Payment dbPayment)
    {
        var dbPlace = dbPayment.PlaceId != null ? context.Places.Single(x => x.UserId == dbUser.Id && x.Id == dbPayment.PlaceId) : null;
        var hasAnyPayments = false;
        if (dbPlace != null)
        {
            hasAnyPayments = IsPlaceBusy(dbPlace.Id, dbUser.Id, dbPayment.Id);
        }

        int? placeId = null;
        if (!string.IsNullOrEmpty(place))
        {
            var dbNewPlace = context.Places.SingleOrDefault(x => x.UserId == dbUser.Id && x.Name == place);
            if (dbNewPlace == null)
            {
                if (dbPlace != null && !hasAnyPayments)
                {
                    dbNewPlace = dbPlace;
                }
                else
                {
                    var newPlaceId = dbUser.NextPlaceId;
                    dbUser.NextPlaceId++;

                    dbNewPlace = new Data.Entities.Place
                    {
                        Name = "",
                    };
                    dbNewPlace.UserId = dbUser.Id;
                    dbNewPlace.Id = newPlaceId;
                    context.Places.Add(dbNewPlace);
                }
                dbNewPlace.LastUsedDate = DateTime.Now;
            }
            else
            {
                dbNewPlace.LastUsedDate = DateTime.Now;
                if (dbPlace != null && !hasAnyPayments && dbPlace.Id != dbNewPlace.Id)
                {
                    dbPlace.IsDeleted = true;
                }
            }

            dbNewPlace.IsDeleted = false;
            dbNewPlace.Name = place;
            placeId = dbNewPlace.Id;
        }
        else
        {
            placeId = null;
            if (dbPlace != null && !hasAnyPayments)
            {
                dbPlace.IsDeleted = true;
            }
        }

        return placeId;
    }

    public bool IsPlaceBusy(int placeId, int userId, int? paymentId)
    {
        var hasAnyPayments = false;
        if (paymentId != null)
        {
            hasAnyPayments = context.Payments.Any(x => x.UserId == userId && x.PlaceId == placeId && x.Id != paymentId);
        }
        else
        {
            hasAnyPayments = context.Payments.Any(x => x.UserId == userId && x.PlaceId == placeId);
        }
        return hasAnyPayments;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Data.Entities.Payment dbPayment = await GetByIdInternal(id, cancellationToken);
        dbPayment.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        Data.Entities.Payment dbPayment = await context.Payments.IgnoreQueryFilters()
                                                .Where(x => x.IsDeleted)
                                                .SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                            ?? throw new NotFoundException("Извините, но платеж не найден. Пожалуйста, проверьте правильность введенных данных.");
        dbPayment.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);
    }
}
