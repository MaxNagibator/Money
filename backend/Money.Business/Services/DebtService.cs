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

        Validate(debt);

        var dbUser = await userService.GetCurrent(cancellationToken);

        var debtId = dbUser.NextDebtId;
        dbUser.NextDebtId++;

        var debtUser = await GetDebtUserAsync(debt, dbUser);

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

    private async Task<Data.Entities.DebtUser> GetDebtUserAsync(Debt debt, Data.Entities.DomainUser? dbUser = null, CancellationToken cancellationToken = default)
    {
        if (dbUser == null)
        {
            dbUser = await userService.GetCurrent(cancellationToken);
        }
        var debtUser = await context.DebtUsers.IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Name == debt.DebtUserName, cancellationToken);

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

        return debtUser;
    }

    private void Validate(Debt debt)
    {
        if (debt.Sum < 0)
        {
            throw new BusinessException("Извините, но отрицательная сумма недопустима");
        }

    }

    public async Task UpdateAsync(Debt debt, CancellationToken cancellationToken)
    {
        Validate(debt);
        var dbDebt = await context.Debts
                              .IsUserEntity(environment.UserId, debt.Id)
                              .FirstOrDefaultAsync(cancellationToken)
                          ?? throw new NotFoundException("Долг", debt.Id);
        if (dbDebt.StatusId != (int)DebtStatus.Actual)
        {
            throw new BusinessException("Извините, но можно обновлять только непогашенные долги");
        }
        if (dbDebt.PaySum > 0 && dbDebt.PaySum >= debt.Sum)
        {
            throw new BusinessException("Извините, но сумма оплаты долга не может превышать сумму долга");
        }
        var debtUser = await GetDebtUserAsync(debt, null, cancellationToken);

        dbDebt.Sum = debt.Sum;
        dbDebt.DebtUserId = debtUser.Id;
        dbDebt.Comment = debt.Comment;
        dbDebt.Date = debt.Date;
        dbDebt.TypeId = (int)debt.Type;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var dbDebt = await GetByIdInternal(id, cancellationToken);
        dbDebt.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        var dbOperation = await context.Debts
                              .IgnoreQueryFilters()
                              .Where(x => x.IsDeleted)
                              .IsUserEntity(environment.UserId, id)
                              .FirstOrDefaultAsync(cancellationToken)
                          ?? throw new NotFoundException("Долг", id);

        dbOperation.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Data.Entities.Debt> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        var dbCategory = await context.Debts
                             .IsUserEntity(environment.UserId, id)
                             .FirstOrDefaultAsync(cancellationToken)
                         ?? throw new NotFoundException("Долг", id);

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
