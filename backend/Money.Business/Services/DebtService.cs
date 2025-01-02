using Money.Data.Extensions;

namespace Money.Business.Services;

public class DebtService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService)
{
    public async Task<IEnumerable<Debt>> GetAsync(bool withPaid = false, CancellationToken cancellationToken = default)
    {
        var query = context.Debts.IsUserEntity(environment.UserId);

        if (withPaid)
        {
            query = query.Where(x => x.StatusId == (int)DebtStatus.Actual || x.StatusId == (int)DebtStatus.Paid);
        }
        else
        {
            query = query.Where(x => x.StatusId == (int)DebtStatus.Actual);
        }

        var dbDebts = await query.ToListAsync(cancellationToken);

        var dbDebtUsers = await context.DebtUsers
            .IsUserEntity(environment.UserId)
            .ToListAsync(cancellationToken);

        var categories = dbDebts
            .Select(x => GetBusinessModel(x, dbDebtUsers))
            .ToList();

        return categories;
    }

    public async Task<Debt> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var dbDebt = await GetByIdInternal(id, cancellationToken);

        var dbDebtUsers = await context.DebtUsers
            .IsUserEntity(environment.UserId)
            .Where(x => x.Id == dbDebt.DebtUserId)
            .ToListAsync(cancellationToken);

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

        var dbUser = await userService.GetCurrent(cancellationToken);

        var debtId = dbUser.NextDebtId;
        dbUser.NextDebtId++;

        var debtUser = context.DebtUsers.IsUserEntity(environment.UserId)
            .FirstOrDefault(x => x.Name == debt.DebtUserName);

        if (debtUser == null)
        {
            var debtUserId = dbUser.NextDebtUserId;
            dbUser.NextDebtUserId++;

            debtUser = new()
            {
                Name = debt.DebtUserName,
                UserId = environment.UserId.Value,
                Id = debtUserId,
            };

            context.DebtUsers.Add(debtUser);
        }

        var dbDebt = new Data.Entities.Debt
        {
            Id = debtId,
            UserId = environment.UserId.Value,
            Sum = debt.Sum,
            TypeId = (int)debt.Type,
            DebtUserId = debtUser.Id,
            Comment = debt.Comment,
            Date = debt.Date,
            PaySum = 0,
            StatusId = (int)DebtStatus.Actual,
        };

        await context.Debts.AddAsync(dbDebt, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return debtId;
    }

    private async Task<Data.Entities.Debt> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        var dbCategory = await context.Debts
                             .IsUserEntity(environment.UserId, id)
                             .FirstOrDefaultAsync(cancellationToken)
                         ?? throw new NotFoundException("долг", id);

        return dbCategory;
    }

    private Debt GetBusinessModel(Data.Entities.Debt dbDebt, IEnumerable<Data.Entities.DebtUser> dbDebtUsers)
    {
        var dbDebtUser = dbDebtUsers.Single(x => x.Id == dbDebt.DebtUserId);

        return new()
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
