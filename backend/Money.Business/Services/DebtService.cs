using Money.Data.Extensions;

namespace Money.Business.Services;

public class DebtService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UserService userService,
    CategoryService categoryService,
    OperationService operationService)
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

        var dbDebtOwners = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .ToListAsync(cancellationToken);

        var categories = dbDebts
            .Select(x => GetBusinessModel(x, dbDebtOwners))
            .ToList();

        return categories;
    }

    public async Task<Debt> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var dbDebt = await GetByIdInternal(id, cancellationToken);

        var dbDebtOwners = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .Where(x => x.Id == dbDebt.OwnerId)
            .ToListAsync(cancellationToken);

        return GetBusinessModel(dbDebt, dbDebtOwners);
    }

    public async Task<int> CreateAsync(Debt debt, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        Validate(debt);

        var debtId = await userService.GetNextDebtIdAsync(cancellationToken);

        var debtOwner = await GetOwnerAsync(debt, cancellationToken);

        var dbDebt = new Data.Entities.Debt
        {
            Id = debtId,
            UserId = environment.UserId.Value,
            Sum = debt.Sum,
            TypeId = (int)debt.Type,
            OwnerId = debtOwner.Id,
            Comment = debt.Comment,
            Date = debt.Date,
            PaySum = 0,
            StatusId = (int)DebtStatus.Actual,
        };

        await context.Debts.AddAsync(dbDebt, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return debtId;
    }

    private async Task<Data.Entities.DebtOwner> GetOwnerAsync(Debt debt, CancellationToken cancellationToken = default)
    {
        var debtOwner = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Name == debt.OwnerName, cancellationToken);

        if (debtOwner == null)
        {
            debtOwner = new()
            {
                Name = debt.OwnerName,
                UserId = environment.UserId!.Value,
                Id = await userService.GetNextDebtOwnerIdAsync(cancellationToken),
            };

            await context.DebtOwners.AddAsync(debtOwner, cancellationToken);
        }

        return debtOwner;
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

        if (dbDebt.PaySum > 0 && debt.Sum <= dbDebt.PaySum)
        {
            throw new BusinessException("Извините, но сумма долга не может быть меньше оплаченной части долга");
        }

        var debtOwner = await GetOwnerAsync(debt, cancellationToken);

        dbDebt.Sum = debt.Sum;
        dbDebt.OwnerId = debtOwner.Id;
        dbDebt.Comment = debt.Comment;
        dbDebt.Date = debt.Date;
        dbDebt.TypeId = (int)debt.Type;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task PayAsync(DebtPayment debtPayment, CancellationToken cancellationToken)
    {
        var dbDebt = await GetByIdInternal(debtPayment.Id, cancellationToken);

        if (dbDebt.PaySum + debtPayment.Sum > dbDebt.Sum)
        {
            throw new BusinessException("Извините, но оплаченная часть долга не может превышать сумму долга");
        }

        dbDebt.PaySum += debtPayment.Sum;

        if (dbDebt.PaySum == dbDebt.Sum)
        {
            dbDebt.StatusId = (int)DebtStatus.Paid;
        }

        if (string.IsNullOrEmpty(dbDebt.PayComment) == false)
        {
            dbDebt.PayComment += Environment.NewLine;
        }

        dbDebt.PayComment += $"{debtPayment.Date:yyyy.MM.dd} {debtPayment.Sum} {debtPayment.Comment}";
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MergeOwnersAsync(int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        if (fromUserId == toUserId)
        {
            throw new BusinessException("Нужно выбрать разных держателей долга");
        }

        var dbFromUser = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Id == fromUserId, cancellationToken);

        if (dbFromUser == null)
        {
            throw new BusinessException("Сливаемый не найден");
        }

        var dbToUser = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Id == toUserId, cancellationToken);

        if (dbToUser == null)
        {
            throw new BusinessException("Поглощающий не найден");
        }

        var dbDebts = await context.Debts
            .IsUserEntity(environment.UserId)
            .Where(x => x.OwnerId == dbFromUser.Id)
            .ToListAsync(cancellationToken);

        foreach (var dbDebt in dbDebts)
        {
            dbDebt.OwnerId = toUserId;
        }

        context.DebtOwners.Remove(dbFromUser);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<DebtOwner>> GetOwnersAsync(CancellationToken cancellationToken)
    {
        var dbOwnerIdsByDebts = context.Debts
            .IsUserEntity(environment.UserId)
            .Select(x => x.OwnerId);

        var dbDebtUsers = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .Where(x => dbOwnerIdsByDebts.Contains(x.Id))
            .Select(dbDebtUser => new DebtOwner
            {
                Id = dbDebtUser.Id,
                Name = dbDebtUser.Name,
            })
            .ToListAsync(cancellationToken);

        return dbDebtUsers;
    }

    public async Task ForgiveAsync(int[] debtIds, int operationCategoryId, string? operationComment, CancellationToken cancellationToken)
    {
        var dbDebts = await context.Debts
            .IsUserEntity(environment.UserId)
            .Where(x => debtIds.Contains(x.Id) && x.StatusId == (int)DebtStatus.Actual)
            .ToListAsync(cancellationToken);

        if (dbDebts.Count == 0)
        {
            throw new BusinessException("Ни один долг не найден или статус долга не соответствует");
        }

        if (dbDebts.Any(x => x.TypeId == (int)DebtTypes.Minus))
        {
            throw new BusinessException("Перемещать можно только долги, которые должны вам");
        }

        var category = await categoryService.GetByIdAsync(operationCategoryId, cancellationToken);

        if (category.OperationType != OperationTypes.Costs)
        {
            throw new BusinessException("Перемещать можно только в категорию расходов");
        }

        var dbOwners = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .ToDictionaryAsync(owner => owner.Id, cancellationToken);

        // TODO: завести мидлварку с UOW и транзакциями
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var dbDebt in dbDebts)
            {
                var dbOwner = dbOwners[dbDebt.OwnerId];
                var comment = $"{operationComment} {dbOwner.Name} сумма долга: {dbDebt.Sum} из них оплачено: {dbDebt.PaySum}";

                if (string.IsNullOrWhiteSpace(dbDebt.Comment) == false)
                {
                    comment += Environment.NewLine + $"комментарий: {dbDebt.Comment}";
                }

                if (string.IsNullOrWhiteSpace(dbDebt.PayComment) == false)
                {
                    comment += Environment.NewLine + $"платёжный комментарий: {dbDebt.PayComment}";
                }

                var operation = new Operation
                {
                    CategoryId = category.Id,
                    Comment = comment,
                    Date = dbDebt.Date,
                    Sum = dbDebt.Sum - dbDebt.PaySum,
                };

                await operationService.CreateAsync(operation, cancellationToken);
                context.Debts.Remove(dbDebt);
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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

    private Debt GetBusinessModel(Data.Entities.Debt dbDebt, IEnumerable<Data.Entities.DebtOwner> dbOwners)
    {
        var dbOwner = dbOwners.First(x => x.Id == dbDebt.OwnerId);

        return new()
        {
            Type = (DebtTypes)dbDebt.TypeId,
            Sum = dbDebt.Sum,
            OwnerName = dbOwner.Name,
            Comment = dbDebt.Comment,
            Id = dbDebt.Id,
            Date = dbDebt.Date,
            PaySum = dbDebt.PaySum,
            PayComment = dbDebt.PayComment,
            IsDeleted = dbDebt.IsDeleted,
        };
    }
}
