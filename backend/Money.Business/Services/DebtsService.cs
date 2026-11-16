using Money.Data.Extensions;

namespace Money.Business.Services;

public class DebtsService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UsersService usersService,
    CategoriesService categoriesService,
    OperationsService operationsService)
{
    public async Task<IEnumerable<Debt>> GetAsync(bool withPaid = false, CancellationToken cancellationToken = default)
    {
        var query = context.Debts.IsUserEntity(environment.UserId);

        query = withPaid
            ? query.Where(x => x.StatusId == (int)DebtStatus.Actual || x.StatusId == (int)DebtStatus.Paid)
            : query.Where(x => x.StatusId == (int)DebtStatus.Actual);

        var entities = await query.ToListAsync(cancellationToken);

        var dbDebtOwners = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .ToListAsync(cancellationToken);

        var models = entities
            .Select(x => GetBusinessModel(x, dbDebtOwners))
            .ToList();

        return models;
    }

    public async Task<Debt> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdInternal(id, cancellationToken);

        var dbDebtOwners = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .Where(x => x.Id == entity.OwnerId)
            .ToListAsync(cancellationToken);

        return GetBusinessModel(entity, dbDebtOwners);
    }

    public async Task<int> CreateAsync(Debt model, CancellationToken cancellationToken = default)
    {
        Validate(model);

        var id = await usersService.GetNextDebtIdAsync(cancellationToken);
        var debtOwner = await GetOwnerAsync(model, cancellationToken);

        var entity = new Data.Entities.Debt
        {
            Id = id,
            UserId = environment.UserId,
            Sum = model.Sum,
            TypeId = (int)model.Type,
            OwnerId = debtOwner.Id,
            Comment = model.Comment,
            Date = model.Date,
            PaySum = 0,
            StatusId = (int)DebtStatus.Actual,
        };

        await context.Debts.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateAsync(Debt model, CancellationToken cancellationToken = default)
    {
        Validate(model);

        var entity = await GetByIdInternal(model.Id, cancellationToken);

        if (entity.StatusId != (int)DebtStatus.Actual)
        {
            throw new BusinessException("Извините, но можно обновлять только непогашенные долги");
        }

        if (entity.PaySum > 0 && model.Sum <= entity.PaySum)
        {
            throw new BusinessException("Извините, но сумма долга не может быть меньше оплаченной части долга");
        }

        var debtOwner = await GetOwnerAsync(model, cancellationToken);

        entity.Sum = model.Sum;
        entity.OwnerId = debtOwner.Id;
        entity.Comment = model.Comment;
        entity.Date = model.Date;
        entity.TypeId = (int)model.Type;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task PayAsync(DebtPayment debtPayment, CancellationToken cancellationToken = default)
    {
        ValidatePayment(debtPayment);
        await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            var entity = await GetByIdInternal(debtPayment.Id, cancellationToken);
            if (debtPayment.Sum > entity.Sum - entity.PaySum)
            {
                throw new BusinessException("Извините, но оплаченная часть долга не может превышать сумму долга");
            }

            ApplyPayment(entity, debtPayment, debtPayment.Sum);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (exception.GetBaseException() is Npgsql.PostgresException { SqlState: "40001" })
        {
            throw new BusinessException("Извините, но долги изменились, обновите страницу перед возвратом");
        }
    }

    public async Task PayOwnerAsync(int debtId, DebtOwnerPayment payment, CancellationToken cancellationToken = default)
    {
        ValidatePayment(payment);
        if (Enum.IsDefined(payment.Type) == false)
        {
            throw new BusinessException("Извините, неподдерживаемый тип долга");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            var source = await GetByIdInternal(debtId, cancellationToken);
            var sourceOwner = await context.DebtOwners
                .IsUserEntity(environment.UserId)
                .FirstOrDefaultAsync(x => x.Id == source.OwnerId, cancellationToken);

            if (source.StatusId != (int)DebtStatus.Actual
                || source.Sum <= source.PaySum
                || source.TypeId != (int)payment.Type
                || sourceOwner == null
                || string.Equals(sourceOwner.Name, payment.OwnerName, StringComparison.Ordinal) == false)
            {
                throw new BusinessException("Извините, но долги изменились, обновите страницу перед возвратом");
            }

            var ownerIds = context.DebtOwners
                .IsUserEntity(environment.UserId)
                .Where(x => x.Name == sourceOwner.Name)
                .Select(x => x.Id);

            var entries = await context.Debts.IsUserEntity(environment.UserId)
                .Where(x => ownerIds.Contains(x.OwnerId) && x.TypeId == source.TypeId
                    && x.StatusId == (int)DebtStatus.Actual && x.Sum > x.PaySum)
                .OrderBy(x => x.Date).ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);
            var available = entries.Sum(x => x.Sum - x.PaySum);
            if (payment.Sum > available)
            {
                throw new BusinessException($"Извините, но сумма возврата превышает остаток. Максимальная сумма: {available}");
            }

            var remaining = payment.Sum;
            foreach (var entry in entries)
            {
                var part = Math.Min(remaining, entry.Sum - entry.PaySum);
                ApplyPayment(entry, payment, part);
                remaining -= part;
                if (remaining == 0)
                {
                    break;
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (exception.GetBaseException() is Npgsql.PostgresException { SqlState: "40001" })
        {
            throw new BusinessException("Извините, но долги изменились, обновите страницу перед возвратом");
        }
    }

    private static void ValidatePayment(DebtPayment payment)
    {
        if (payment.Sum <= 0 || payment.Date == default)
        {
            throw new BusinessException("Извините, но нужно указать дату и положительную сумму возврата");
        }
    }

    private static void ApplyPayment(Money.Data.Entities.Debt entity, DebtPayment payment, decimal sum)
    {
        var line = $"{payment.Date:yyyy.MM.dd} {sum} {payment.Comment}";
        var history = string.IsNullOrEmpty(entity.PayComment) ? line : entity.PayComment + Environment.NewLine + line;
        if (history.Length > 4000)
        {
            throw new BusinessException("Извините, но история оплат вместе с комментарием слишком длинная");
        }

        entity.PayComment = history;
        entity.PaySum += sum;
        if (entity.PaySum == entity.Sum)
        {
            entity.StatusId = (int)DebtStatus.Paid;
        }
    }

    public async Task MergeOwnersAsync(int fromUserId, int toUserId, CancellationToken cancellationToken = default)
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

        foreach (var entity in dbDebts)
        {
            entity.OwnerId = toUserId;
        }

        context.DebtOwners.Remove(dbFromUser);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<DebtOwner>> GetOwnersAsync(CancellationToken cancellationToken = default)
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

    public Task<List<DebtOwner>> GetOwnersAsync(int offset, int count, string? name, CancellationToken cancellationToken = default)
    {
        var dbOwnerIdsByDebts = context.Debts
            .IsUserEntity(environment.UserId)
            .Select(x => x.OwnerId);

        var query = context.DebtOwners
            .IsUserEntity(environment.UserId)
            .Where(x => dbOwnerIdsByDebts.Contains(x.Id));

        var hasFilter = string.IsNullOrWhiteSpace(name) == false;

        if (hasFilter)
        {
            var containsPattern = $"%{EscapeLikePattern(name!)}%";
            query = query.Where(x => EF.Functions.ILike(x.Name, containsPattern, "\\"));
        }

        IOrderedQueryable<Data.Entities.DebtOwner> ordered;

        if (hasFilter)
        {
            var startsWithPattern = $"{EscapeLikePattern(name!)}%";
            ordered = query
                .OrderByDescending(x => EF.Functions.ILike(x.Name, startsWithPattern, "\\"))
                .ThenBy(x => x.Name);
        }
        else
        {
            ordered = query.OrderBy(x => x.Name);
        }

        return ordered
            .Skip(offset)
            .Take(count)
            .Select(x => new DebtOwner
            {
                Id = x.Id,
                Name = x.Name,
            })
            .ToListAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }

    public async Task ForgiveAsync(int[] debtIds, int operationCategoryId, string? operationComment, CancellationToken cancellationToken = default)
    {
        var entities = await context.Debts
            .IsUserEntity(environment.UserId)
            .Where(x => debtIds.AsEnumerable().Contains(x.Id) && x.StatusId == (int)DebtStatus.Actual)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
        {
            throw new BusinessException("Ни один долг не найден или статус долга не соответствует");
        }

        if (entities.Any(x => x.TypeId == (int)DebtTypes.Minus))
        {
            throw new BusinessException("Перемещать можно только долги, которые должны вам");
        }

        var category = await categoriesService.GetByIdAsync(operationCategoryId, cancellationToken);

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
            foreach (var entity in entities)
            {
                var dbOwner = dbOwners[entity.OwnerId];
                var comment = $"{operationComment} {dbOwner.Name} сумма долга: {entity.Sum} из них оплачено: {entity.PaySum}";

                if (string.IsNullOrWhiteSpace(entity.Comment) == false)
                {
                    comment += Environment.NewLine + $"комментарий: {entity.Comment}";
                }

                if (string.IsNullOrWhiteSpace(entity.PayComment) == false)
                {
                    comment += Environment.NewLine + $"платёжный комментарий: {entity.PayComment}";
                }

                var operation = new Operation
                {
                    CategoryId = category.Id,
                    Comment = comment,
                    Date = entity.Date,
                    Sum = entity.Sum - entity.PaySum,
                };

                await operationsService.CreateAsync(operation, cancellationToken);
                context.Debts.Remove(entity);
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
        var entity = await GetByIdInternal(id, cancellationToken);
        entity.IsDeleted = true;
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

    private static Debt GetBusinessModel(Data.Entities.Debt entity, IEnumerable<Data.Entities.DebtOwner> dbOwners)
    {
        var dbOwner = dbOwners.First(x => x.Id == entity.OwnerId);

        return new()
        {
            Type = (DebtTypes)entity.TypeId,
            Sum = entity.Sum,
            OwnerName = dbOwner.Name,
            Comment = entity.Comment,
            Id = entity.Id,
            Date = entity.Date,
            PaySum = entity.PaySum,
            PayComment = entity.PayComment,
            IsDeleted = entity.IsDeleted,
            Status = (DebtStatus)entity.StatusId,
        };
    }

    private static void Validate(Debt model)
    {
        if (model.Date == default)
        {
            throw new BusinessException("Извините, но дата обязательна");
        }

        if (model.Sum <= 0)
        {
            throw new BusinessException("Извините, но сумма должна быть больше нуля");
        }

        if (Enum.IsDefined(model.Type) == false)
        {
            throw new BusinessException("Извините, неподдерживаемый тип долга");
        }

        if (model.Comment?.Length > 4000)
        {
            throw new BusinessException("Извините, но комментарий слишком длинный");
        }

        if (model.OwnerName.Length > 500)
        {
            throw new BusinessException("Извините, но имя держателя слишком длинное");
        }
    }

    public async Task ForgiveAccountAsync(int debtId, int categoryId, decimal expectedBalance, DateTime date, string? comment, CancellationToken cancellationToken = default)
    {
        if (date == default || expectedBalance <= 0)
        {
            throw new BusinessException("Извините, но нужно указать дату и положительный остаток");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            var source = await GetByIdInternal(debtId, cancellationToken);
            var entries = await context.Debts.IsUserEntity(environment.UserId)
                .Where(x => x.OwnerId == source.OwnerId)
                .ToListAsync(cancellationToken);
            var balance = entries.Sum(x => (x.TypeId == (int)DebtTypes.Plus ? 1 : -1) * (x.Sum - x.PaySum));
            if (balance <= 0 || balance != expectedBalance)
            {
                throw new BusinessException("Извините, но остаток изменился, обновите расчёты перед списанием");
            }

            var category = await categoriesService.GetByIdAsync(categoryId, cancellationToken);
            if (category.OperationType != OperationTypes.Costs)
            {
                throw new BusinessException("Извините, но списать остаток можно только в категорию расходов");
            }

            var owner = await context.DebtOwners.IsUserEntity(environment.UserId, source.OwnerId).FirstAsync(cancellationToken);
            var expenseComment = $"Списание долга: {owner.Name}. {comment}";
            if (expenseComment.Length > 4000)
            {
                throw new BusinessException("Извините, но комментарий вместе с именем держателя слишком длинный");
            }

            context.Debts.RemoveRange(entries);
            await operationsService.CreateAsync(new()
            {
                CategoryId = category.Id,
                Sum = balance,
                Date = date.Date,
                Comment = expenseComment,
            }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (exception.GetBaseException() is Npgsql.PostgresException { SqlState: "40001" or "23505" })
        {
            throw new BusinessException("Извините, но расчёты изменились, обновите страницу перед списанием");
        }
    }

    private async Task<Data.Entities.DebtOwner> GetOwnerAsync(Debt model, CancellationToken cancellationToken = default)
    {
        var debtOwner = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Name == model.OwnerName, cancellationToken);

        if (debtOwner == null)
        {
            debtOwner = new()
            {
                Name = model.OwnerName,
                UserId = environment.UserId,
                Id = await usersService.GetNextDebtOwnerIdAsync(cancellationToken),
            };

            await context.DebtOwners.AddAsync(debtOwner, cancellationToken);
        }

        return debtOwner;
    }

    private async Task<Data.Entities.Debt> GetByIdInternal(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Debts
                         .IsUserEntity(environment.UserId, id)
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException("Долг", id);

        return entity;
    }
}
