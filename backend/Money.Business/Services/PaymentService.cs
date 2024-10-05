using Microsoft.EntityFrameworkCore;
using Money.Business.Models;
using Money.Data;
using Money.Data.Extensions;

namespace Money.Business.Services;

public class PaymentService(RequestEnvironment environment, ApplicationDbContext context)
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
                .Select(x => x.PlaceId);

            dbPayments = dbPayments.Where(x => x.PlaceId != null && placesIds.Contains(x.PlaceId.Value));
        }

        return dbPayments;
    }

    private async Task<List<Data.Entities.Place>> GetPlacesAsync(List<int> placeIds, CancellationToken cancellationToken)
    {
        return await context.Places
            .Where(x => x.UserId == environment.UserId && placeIds.Contains(x.PlaceId))
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
                ? dbPlaces.FirstOrDefault(x => x.PlaceId == dbPayment.PlaceId)?.Name
                : null,
            Id = dbPayment.Id,
            Date = dbPayment.Date,
            CreatedTaskId = dbPayment.CreatedTaskId,
        };
    }
}
