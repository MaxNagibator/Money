using Money.Data.Extensions;

namespace Money.Business.Services;

public class DebtService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService)
{
    public async Task<IEnumerable<Debt>> GetAsync(bool withPaid = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Data.Entities.Debt> query = context.Debts.IsUserEntity(environment.UserId);

        if (withPaid)
        {
            query = query.Where(x => x.StatusId == (int)DebtStatus.Actual || x.StatusId == (int)DebtStatus.Paid);
        }
        else
        {
            query = query.Where(x => x.StatusId == (int)DebtStatus.Actual);
        }

        var dbDebts = await query.ToListAsync(cancellationToken);
        var dbDebtUsers = await context.DebtUsers.IsUserEntity(environment.UserId).ToListAsync(cancellationToken);

        List<Debt> categories = dbDebts
            .Select(x => GetBusinessModel(x, dbDebtUsers))
            .ToList();

        return categories;
    }

    private Debt GetBusinessModel(Data.Entities.Debt dbDebt, IEnumerable<Data.Entities.DebtUser> dbDebtUsers)
    {
        var dbDebtUser = dbDebtUsers.Single(x => x.Id == dbDebt.DebtUserId);
        return new Debt
        {
            Type = (DebtTypes)dbDebt.TypeId,
            Sum = dbDebt.Sum,
            DebtUserName = dbDebtUser.Name,
            Comment = dbDebt.Comment,
            Id = dbDebt.Id,
            Date = dbDebt.Date,
            PaySum = dbDebt.PaySum,
            PayComment = dbDebt.PayComment,
        };
    }
}
