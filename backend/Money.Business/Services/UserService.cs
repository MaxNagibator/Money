namespace Money.Business.Services;

public class UserService(RequestEnvironment environment, ApplicationDbContext context)
{
    private DomainUser? _currentUser;

    public async Task<DomainUser> GetCurrent(CancellationToken cancellationToken)
    {
        return _currentUser ??= await context.DomainUsers.FirstOrDefaultAsync(x => x.Id == environment.UserId, cancellationToken)
                                ?? throw new BusinessException("Извините, но пользователь не найден.");
    }
}
