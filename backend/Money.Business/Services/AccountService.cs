using Microsoft.AspNetCore.Identity;

namespace Money.Business.Services;

public class AccountService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
{
    public async Task RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByNameAsync(model.Email);

        if (user != null)
        {
            throw new EntityExistsException("Извините, но пользователь с таким именем уже зарегистрирован. Пожалуйста, попробуйте другое имя пользователя.");
        }

        user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
        };

        IdentityResult result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded == false)
        {
            throw new IncorrectDataException($"Ошибки: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }

        await AddNewUser(user.Id, cancellationToken);
    }

    public async Task<int> EnsureUserIdAsync(Guid authUserId, CancellationToken cancellationToken = default)
    {
        DomainUser? domainUser = await context.DomainUsers.FirstOrDefaultAsync(x => x.AuthUserId == authUserId, cancellationToken);

        if (domainUser != null)
        {
            return domainUser.Id;
        }

        return await AddNewUser(authUserId, cancellationToken);
    }

    private async Task<int> AddNewUser(Guid authUserId, CancellationToken cancellationToken)
    {
        DomainUser domainUser = new()
        {
            AuthUserId = authUserId,
            NextCategoryId = 1,
            NextOperationId = 1,
            NextFastOperationId = 1,
            NextRegularOperationId = 1,
            NextPlaceId = 1,
        };

        await context.DomainUsers.AddAsync(domainUser, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return domainUser.Id;
    }
}
