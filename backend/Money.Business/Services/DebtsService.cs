using Money.Data.Extensions;

namespace Money.Business.Services;

public class DebtsService(
    RequestEnvironment environment,
    ApplicationDbContext context,
    UsersService usersService,
    CategoriesService categoriesService,
    OperationsService operationsService)
{
    public async Task<IEnumerable<DebtMovement>> GetAsync(bool withPaid = false, CancellationToken cancellationToken = default)
    {
        _ = withPaid; // Совместимость старого API: закрытые счета фильтруются по сумме движений на клиенте.
        var entities = await context.DebtMovements
            .IsUserEntity(environment.UserId)
            .ToListAsync(cancellationToken);

        var dbDebtOwners = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .ToListAsync(cancellationToken);

        var models = entities
            .Select(x => GetBusinessModel(x, dbDebtOwners))
            .ToList();

        return models;
    }

    public async Task<DebtMovement> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdInternal(id, cancellationToken);

        var dbDebtOwners = await context.DebtOwners
            .IsUserEntity(environment.UserId)
            .Where(x => x.Id == entity.OwnerId)
            .ToListAsync(cancellationToken);

        return GetBusinessModel(entity, dbDebtOwners);
    }

    public async Task<int> CreateAsync(DebtMovement model, CancellationToken cancellationToken = default)
    {
        Validate(model);

        var id = await usersService.GetNextDebtIdAsync(cancellationToken);
        var debtOwner = await GetOwnerAsync(model, cancellationToken);

        var entity = new Data.Entities.DebtMovement
        {
            Id = id,
            UserId = environment.UserId,
            Sum = model.Sum,
            TypeId = (int)model.Type,
            OwnerId = debtOwner.Id,
            Comment = model.Comment,
            Date = model.Date,
        };

        await context.DebtMovements.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateAsync(DebtMovement model, CancellationToken cancellationToken = default)
    {
        Validate(model);

        var entity = await GetByIdInternal(model.Id, cancellationToken);

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
        var entity = await GetByIdInternal(debtPayment.Id, cancellationToken);

        if (debtPayment.Sum <= 0)
        {
            throw new BusinessException("Извините, но сумма должна быть больше нуля");
        }

        if (debtPayment.Date == default)
        {
            throw new BusinessException("Извините, но дата обязательна");
        }

        if (debtPayment.Comment?.Length > 4000)
        {
            throw new BusinessException("Извините, но комментарий слишком длинный");
        }

        var ownerMovements = await context.DebtMovements
            .IsUserEntity(environment.UserId)
            .Where(x => x.OwnerId == entity.OwnerId)
            .ToListAsync(cancellationToken);

        var balance = ownerMovements.Sum(x => x.TypeId == (int)DebtTypes.Plus
            ? x.Sum
            : -x.Sum);
        var payableRemainder = entity.TypeId == (int)DebtTypes.Plus ? balance : -balance;

        if (payableRemainder <= 0)
        {
            throw new BusinessException("Извините, но выбранное направление уже погашено встречными движениями");
        }

        if (debtPayment.Sum > payableRemainder)
        {
            throw new BusinessException("Извините, но выплата не может превышать остаток по держателю");
        }

        // Возврат — самостоятельное встречное движение. Старые записи не
        // мутируются, поэтому историю можно редактировать и удалять без потери
        // происхождения текущего баланса.
        var paymentMovement = new Data.Entities.DebtMovement
        {
            Id = await usersService.GetNextDebtIdAsync(cancellationToken),
            UserId = environment.UserId,
            OwnerId = entity.OwnerId,
            TypeId = entity.TypeId == (int)DebtTypes.Plus
                ? (int)DebtTypes.Minus
                : (int)DebtTypes.Plus,
            Sum = debtPayment.Sum,
            Date = debtPayment.Date,
            Comment = debtPayment.Comment,
        };

        await context.DebtMovements.AddAsync(paymentMovement, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
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

        var dbDebts = await context.DebtMovements
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
        var dbOwnerIdsByDebts = context.DebtMovements
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
        var dbOwnerIdsByDebts = context.DebtMovements
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
        var entities = await context.DebtMovements
            .IsUserEntity(environment.UserId)
            .Where(x => debtIds.AsEnumerable().Contains(x.Id))
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

        var selectedOwnerIds = entities.Select(x => x.OwnerId).Distinct().ToArray();
        var ownerMovements = await context.DebtMovements
            .IsUserEntity(environment.UserId)
            .Where(x => selectedOwnerIds.Contains(x.OwnerId))
            .ToListAsync(cancellationToken);

        // TODO: завести мидлварку с UOW и транзакциями
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var ownerId in selectedOwnerIds)
            {
                var movements = ownerMovements.Where(x => x.OwnerId == ownerId).ToArray();
                var balance = movements.Sum(x => x.TypeId == (int)DebtTypes.Plus
                    ? x.Sum
                    : -x.Sum);

                if (balance <= 0)
                {
                    throw new BusinessException("Прощать можно только положительный остаток счёта");
                }

                var dbOwner = dbOwners[ownerId];
                var date = movements.Max(x => x.Date);
                var comment = $"{operationComment} {dbOwner.Name}, прощён остаток счёта: {balance}";

                var operation = new Operation
                {
                    CategoryId = category.Id,
                    Comment = comment,
                    Date = date,
                    Sum = balance,
                };

                await operationsService.CreateAsync(operation, cancellationToken);

                await context.DebtMovements.AddAsync(new Data.Entities.DebtMovement
                {
                    Id = await usersService.GetNextDebtIdAsync(cancellationToken),
                    UserId = environment.UserId,
                    OwnerId = ownerId,
                    TypeId = (int)DebtTypes.Minus,
                    Sum = balance,
                    Date = date,
                    Comment = $"Прощение долга. {operationComment}".Trim(),
                }, cancellationToken);
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
        var dbOperation = await context.DebtMovements
                              .IgnoreQueryFilters()
                              .Where(x => x.IsDeleted)
                              .IsUserEntity(environment.UserId, id)
                              .FirstOrDefaultAsync(cancellationToken)
                          ?? throw new NotFoundException("Долг", id);

        dbOperation.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);
    }

    private static DebtMovement GetBusinessModel(Data.Entities.DebtMovement entity, IEnumerable<Data.Entities.DebtOwner> dbOwners)
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
            IsDeleted = entity.IsDeleted,
        };
    }

    private static void Validate(DebtMovement model)
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

    private async Task<Data.Entities.DebtOwner> GetOwnerAsync(DebtMovement model, CancellationToken cancellationToken = default)
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

    private async Task<Data.Entities.DebtMovement> GetByIdInternal(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.DebtMovements
                         .IsUserEntity(environment.UserId, id)
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException("Долг", id);

        return entity;
    }
}
