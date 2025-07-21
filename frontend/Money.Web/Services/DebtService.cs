using Money.ApiClient;

namespace Money.Web.Services;

public class DebtService(MoneyClient moneyClient)
{
    public async Task<IEnumerable<Debt>> GetAllAsync(bool withPaid = false)
    {
        var response = await moneyClient.Debts.Get(withPaid);

        return response.Content?
                   .Select(x => new Debt
                   {
                       Id = x.Id,
                       Type = DebtTypes.Values[x.TypeId],
                       Sum = x.Sum,
                       Comment = x.Comment,
                       OwnerName = x.OwnerName,
                       Date = x.Date,
                       PaySum = x.PaySum,
                       PayComment = x.PayComment,
                   })
               ?? [];
    }
}
