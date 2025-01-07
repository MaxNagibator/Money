using Money.Data.Extensions;

namespace Money.Business.Services;

public class DebtService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService)
{
    private const string PaySumMoreSumExceptionText = "Извините, но сумма оплаты долга не может превышать сумму долга";

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

        var debtId = await userService.GetNextDebtIdAsync(cancellationToken);

        var debtUser = await GetDebtUserAsync(debt, cancellationToken);

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

    private async Task<Data.Entities.DebtUser> GetDebtUserAsync(Debt debt, CancellationToken cancellationToken = default)
    {
        var debtUser = await context.DebtUsers
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Name == debt.DebtUserName, cancellationToken);

        if (debtUser == null)
        {
            debtUser = new()
            {
                Name = debt.DebtUserName,
                UserId = environment.UserId.Value,
                Id = await userService.GetNextDebtUserIdAsync(cancellationToken),
            };

            await context.DebtUsers.AddAsync(debtUser, cancellationToken);
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

        var dbDebt = await GetByIdInternal(debt.Id, cancellationToken);

        if (dbDebt.StatusId != (int)DebtStatus.Actual)
        {
            throw new BusinessException("Извините, но можно обновлять только непогашенные долги");
        }

        if (dbDebt.PaySum > 0 && dbDebt.PaySum >= debt.Sum)
        {
            throw new BusinessException(PaySumMoreSumExceptionText);
        }

        var debtUser = await GetDebtUserAsync(debt, cancellationToken);

        dbDebt.Sum = debt.Sum;
        dbDebt.DebtUserId = debtUser.Id;
        dbDebt.Comment = debt.Comment;
        dbDebt.Date = debt.Date;
        dbDebt.TypeId = (int)debt.Type;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task PayAsync(DebtPayment debtPayment, CancellationToken cancellationToken)
    {
        var dbDebt = await GetByIdInternal(debtPayment.Id, cancellationToken);
        if (dbDebt.Sum < dbDebt.PaySum + debtPayment.Sum)
        {
            throw new BusinessException(PaySumMoreSumExceptionText);
        }
        dbDebt.PaySum += debtPayment.Sum;
        if (dbDebt.PaySum == dbDebt.Sum)
        {
            dbDebt.StatusId = (int)DebtStatus.Paid;
        }
        if (!String.IsNullOrEmpty(dbDebt.PayComment))
        {
            dbDebt.PayComment += Environment.NewLine;
        }
        dbDebt.PayComment += debtPayment.Date.ToString("yyyy.MM.dd") + " " + debtPayment.Sum + " " + debtPayment.Comment;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MergeDebtUsersAsync(int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        if (fromUserId == toUserId)
        {
            throw new BusinessException("Нужно выбрать разных держателей долга");
        }

        var dbFromUser = await context.DebtUsers
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Id == fromUserId, cancellationToken);
        if (dbFromUser == null)
        {
            throw new BusinessException("Сливаемый не найден");
        }

        var dbToUser = await context.DebtUsers
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Id == toUserId, cancellationToken);
        if (dbToUser == null)
        {
            throw new BusinessException("Поглощающий не найден");
        }

        var dbDebts = await context.Debts.IsUserEntity(environment.UserId)
            .Where(x => x.DebtUserId == dbFromUser.Id)
            .ToListAsync(cancellationToken);

        foreach (var dbDebt in dbDebts)
        {
            dbDebt.DebtUserId = toUserId;
        }
        context.DebtUsers.Remove(dbFromUser);
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
        var dbDebt = await context.Debts
                             .IsUserEntity(environment.UserId, id)
                             .FirstOrDefaultAsync(cancellationToken)
                         ?? throw new NotFoundException("Долг", id);

        return dbDebt;
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
            IsDeleted = dbDebt.IsDeleted,
        };
    }
}
