namespace Money.Business.Services;

public class UserService(RequestEnvironment environment, ApplicationDbContext context)
{
    private Data.Entities.DomainUser? _currentUser;

    public async Task<Data.Entities.DomainUser> GetCurrent(CancellationToken cancellationToken)
    {
        return _currentUser ??= await context.DomainUsers.FirstOrDefaultAsync(x => x.Id == environment.UserId, cancellationToken)
                                ?? throw new BusinessException("Извините, но пользователь не найден.");
    }
}
