using Microsoft.EntityFrameworkCore;
using Money.Business.Enums;
using Money.Business.Models;
using Money.Data;
using Money.Data.Extensions;

namespace Money.Business.Services
{
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
            if (categoryIds != null && categoryIds.Count > 0)
            {
                dbPayments = dbPayments.Where(x => categoryIds.Contains(x.CategoryId));
            }
            if (!string.IsNullOrEmpty(comment))
            {
                dbPayments = dbPayments.Where(x => x.Comment != null && x.Comment.Contains(comment));
            }
            if (!string.IsNullOrEmpty(place))
            {
                var placesIds = context.Places.Where(x => x.UserId == environment.UserId && x.Name.Contains(place)).Select(x => x.PlaceId);
                dbPayments = dbPayments.Where(x => x.PlaceId != null && placesIds.Contains(x.PlaceId.Value));
            }
            var placeIds = dbPayments.Where(x => x.PlaceId != null).Select(x => x.PlaceId.Value);
            var dbPlaces =  context.Places.Where(x => x.UserId == environment.UserId && placeIds.Contains(x.PlaceId)).ToListAsync();
            var dbPaymentList =  dbPayments.OrderByDescending(x => x.Date).ThenBy(x => x.CategoryId).ToListAsync();
            Task.WaitAll(dbPlaces, dbPaymentList);
            var payments = new List<Payment>();
            foreach (var dbPayment in dbPaymentList.Result)
            {
                payments.Add(MapTo(dbPayment, dbPlaces.Result));
            }
            return payments;
        }

        private Payment MapTo(Data.Entities.Payment dbPayment, List<Data.Entities.Place> dbPlaces)
        {
            var payment = new Payment
            {
                CategoryId = dbPayment.CategoryId,
                Sum = dbPayment.Sum,
                Comment = dbPayment.Comment,
                Place = dbPayment.PlaceId != null ? dbPlaces.First(x => x.PlaceId == dbPayment.PlaceId).Name : null,
                Id = dbPayment.Id,
                Date = dbPayment.Date,
                CreatedTaskId = dbPayment.CreatedTaskId
            };
            return payment;
        }
    }
}
