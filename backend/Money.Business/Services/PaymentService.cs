using Money.Business.Models;
using Money.Data;
using Money.Data.Extensions;

namespace Money.Business.Services
{
    public class PaymentService(RequestEnvironment environment, ApplicationDbContext context)
    {
        public async Task<ICollection<Payment>> GetAsync(CancellationToken cancellationToken = default)
        {
            IQueryable<Data.Entities.Payment> query = context.Payments.IsUserEntity(environment.UserId);
            throw new NotImplementedException();
        }
    }
}
