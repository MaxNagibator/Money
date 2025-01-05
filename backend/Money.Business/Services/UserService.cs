namespace Money.Business.Services;

public class UserService(RequestEnvironment environment, ApplicationDbContext context)
{
    private Data.Entities.DomainUser? _currentUser;

    // todo Бизнес сервис не имеет права отдавать объекты EF наружу
    public async Task<Data.Entities.DomainUser> GetCurrent(CancellationToken cancellationToken)
    {
        // todo контекст и так кэширует полученные данные, а он у нас один на реквест как и сервис
        return _currentUser ??= await context.DomainUsers.FirstOrDefaultAsync(x => x.Id == environment.UserId, cancellationToken)
                                ?? throw new BusinessException("Извините, но пользователь не найден.");
    }
}
