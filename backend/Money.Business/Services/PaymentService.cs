using Microsoft.EntityFrameworkCore;
using Money.Data;
using Money.Data.Entities;
using Money.Data.Extensions;
using Payment = Money.Business.Models.Payment;

namespace Money.Business.Services;

public class PaymentService(RequestEnvironment environment, ApplicationDbContext context)
{
    public async Task<ICollection<Payment>> GetAsync(
        DateTime? dateFrom,
        DateTime? dateTo,
        List<int>? categoryIds,
        string? comment,
        string? place,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Data.Entities.Payment> dbPayments = context.Payments.IsUserEntity(environment.UserId);

        dbPayments = dbPayments.Where(x => x.TaskId == null).AsQueryable();

        if (dateFrom != null)
        {
            dbPayments = dbPayments.Where(x => x.Date >= dateFrom);
        }

        if (dateTo != null)
        {
            dbPayments = dbPayments.Where(x => x.Date < dateTo);
        }

        if (categoryIds is { Count: > 0 })
        {
            dbPayments = dbPayments.Where(x => categoryIds.Contains(x.CategoryId));
        }

        if (!string.IsNullOrEmpty(comment))
        {
            dbPayments = dbPayments.Where(x => x.Comment != null && x.Comment.Contains(comment));
        }

        if (!string.IsNullOrEmpty(place))
        {
            IQueryable<int> placesIds = context.Places.Where(x => x.UserId == environment.UserId && x.Name.Contains(place)).Select(x => x.PlaceId);
            dbPayments = dbPayments.Where(x => x.PlaceId != null && placesIds.Contains(x.PlaceId.Value));
        }

        // A second operation was started on this context instance before a previous operation completed.
        // This is usually caused by different threads concurrently using the same instance of DbContext.
        // For more information on how to avoid threading issues with DbContext,
        // see https://go.microsoft.com/fwlink/?linkid=2097913
        IQueryable<int> placeIds = dbPayments.Where(x => x.PlaceId != null).Select(x => x.PlaceId.Value);
        List<Place> dbPlaces = await context.Places.Where(x => x.UserId == environment.UserId && placeIds.Contains(x.PlaceId)).ToListAsync(cancellationToken: cancellationToken);
        List<Data.Entities.Payment> dbPaymentList = await dbPayments.OrderByDescending(x => x.Date).ThenBy(x => x.CategoryId).ToListAsync(cancellationToken: cancellationToken);

        return dbPaymentList.Select(dbPayment => MapTo(dbPayment, dbPlaces)).ToList();
    }

    private Payment MapTo(Data.Entities.Payment dbPayment, List<Place> dbPlaces)
    {
        Payment payment = new()
        {
            CategoryId = dbPayment.CategoryId,
            Sum = dbPayment.Sum,
            Comment = dbPayment.Comment,
            Place = dbPayment.PlaceId != null ? dbPlaces.First(x => x.PlaceId == dbPayment.PlaceId).Name : null,
            Id = dbPayment.Id,
            Date = dbPayment.Date,
            CreatedTaskId = dbPayment.CreatedTaskId,
        };

        return payment;
    }
}
