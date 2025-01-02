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

    public async Task<Debt> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Data.Entities.Debt dbDebt = await GetByIdInternal(id, cancellationToken);
        var dbDebtUsers = await context.DebtUsers.Where(x => x.UserId == environment.UserId && x.Id == dbDebt.DebtUserId).ToListAsync(cancellationToken);
        return GetBusinessModel(dbDebt, dbDebtUsers);
    }

    public async Task<int> CreateAsync(Debt debt, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        if (debt.Sum < 0)
        {
            throw new BusinessException("Извините, но отрицательная сумма недопустима");
        }

        Data.Entities.DomainUser dbUser = await userService.GetCurrent(cancellationToken);

        int debtId = dbUser.NextDebtId;
        dbUser.NextDebtId++;

        var debtUser = context.DebtUsers.FirstOrDefault(x => x.UserId == environment.UserId && x.Name == debt.DebtUserName);
        if (debtUser == null)
        {
            var debtUserId = dbUser.NextDebtUserId;
            dbUser.NextDebtUserId++;

            debtUser = new Data.Entities.DebtUser
            {
                Name = debt.DebtUserName
            };
            debtUser.UserId = environment.UserId.Value;
            debtUser.Id = debtUserId;
            context.DebtUsers.Add(debtUser);
        }

        Data.Entities.Debt dbDebt = new()
        {
            Id = debtId,
            UserId = environment.UserId.Value,

        };

        dbDebt.Sum = debt.Sum;
        dbDebt.TypeId = (int)debt.Type;
        dbDebt.DebtUserId = debtUser.Id;
        dbDebt.Comment = debt.Comment;
        dbDebt.UserId = environment.UserId.Value;
        dbDebt.Id = debtId;
        dbDebt.Date = debt.Date;
        dbDebt.PaySum = 0;
        dbDebt.StatusId = (int)DebtStatus.Actual;

        await context.Debts.AddAsync(dbDebt, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return debtId;
    }

    private async Task<Data.Entities.Debt> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        Data.Entities.Debt dbCategory = await context.Debts
                                                .IsUserEntity(environment.UserId, id)
                                                .FirstOrDefaultAsync(cancellationToken)
                                            ?? throw new NotFoundException("долг", id);

        return dbCategory;
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
