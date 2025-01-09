namespace Money.Business.Services;

public class UserService(RequestEnvironment environment, ApplicationDbContext context)
{
    private Data.Entities.DomainUser? _currentUser;

    public async Task<int> GetIdAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentAsync(cancellationToken);
        return user.Id;
    }

    public Task<int> GetNextCategoryIdAsync(CancellationToken cancellationToken = default)
    {
        return GetNextIdAsync(x => x.NextCategoryId, x => x.NextCategoryId++, cancellationToken);
    }

    public Task<int> GetNextOperationIdAsync(CancellationToken cancellationToken = default)
    {
        return GetNextIdAsync(x => x.NextOperationId, x => x.NextOperationId++, cancellationToken);
    }

    public Task<int> GetNextPlaceIdAsync(CancellationToken cancellationToken = default)
    {
        return GetNextIdAsync(x => x.NextPlaceId, x => x.NextPlaceId++, cancellationToken);
    }

    public Task<int> GetNextFastOperationIdAsync(CancellationToken cancellationToken = default)
    {
        return GetNextIdAsync(x => x.NextFastOperationId, x => x.NextFastOperationId++, cancellationToken);
    }

    public Task<int> GetNextRegularOperationIdAsync(CancellationToken cancellationToken = default)
    {
        return GetNextIdAsync(x => x.NextRegularOperationId, x => x.NextRegularOperationId++, cancellationToken);
    }

    public Task<int> GetNextDebtIdAsync(CancellationToken cancellationToken = default)
    {
        return GetNextIdAsync(x => x.NextDebtId, x => x.NextDebtId++, cancellationToken);
    }

    public Task<int> GetNextDebtOwnerIdAsync(CancellationToken cancellationToken = default)
    {
        return GetNextIdAsync(x => x.NextDebtOwnerId, x => x.NextDebtOwnerId++, cancellationToken);
    }

    public async Task SetNextCategoryIdAsync(int index, CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentAsync(cancellationToken);
        user.NextCategoryId = index;
    }

    private async Task<Data.Entities.DomainUser> GetCurrentAsync(CancellationToken cancellationToken)
    {
        // TODO: контекст и так кэширует полученные данные, а он у нас один на реквест как и сервис
        return _currentUser ??= await context.DomainUsers.FirstOrDefaultAsync(x => x.Id == environment.UserId, cancellationToken)
                                ?? throw new BusinessException("Извините, но пользователь не найден.");
    }

    private async Task<int> GetNextIdAsync(Func<Data.Entities.DomainUser, int> getId, Action<Data.Entities.DomainUser> incrementId, CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentAsync(cancellationToken);
        var id = getId(user);
        incrementId(user);
        return id;
    }
}
