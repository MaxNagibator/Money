using Microsoft.AspNetCore.Identity;
using Money.Data.Entities;

namespace Money.Business.Services;

public class AccountService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
{
    public async Task RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByNameAsync(model.UserName);

        if (user != null)
        {
            throw new EntityExistsException("Извините, но пользователь с таким именем уже зарегистрирован. Пожалуйста, попробуйте другое имя пользователя.");
        }

        if (model.Email != null)
        {
            user = await userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    throw new EntityExistsException("Извините, но пользователь с таким email уже зарегистрирован. Пожалуйста, попробуйте другое имя пользователя.");
                }
                //else
                //{
                //    userForClearEmail = user;
                //    user.Email = null;
                //    await userManager.UpdateAsync(user);
                //}
            }

        }
        user = new()
        {
            UserName = model.UserName,
            Email = model.Email,
        };

        var result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded == false)
        {
            throw new IncorrectDataException($"Ошибки: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }

        await AddNewUser(user.Id, cancellationToken);

        if(1 > 0)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

    //    if (userForClearEmail != null)
    //    {
    //        userForClearEmail.Email = true;
    //    await userForClearEmail.UpdateAsync(user);
    //}
    }

    public async Task<int> EnsureUserIdAsync(Guid authUserId, CancellationToken cancellationToken = default)
    {
        var domainUser = await context.DomainUsers.FirstOrDefaultAsync(x => x.AuthUserId == authUserId, cancellationToken);

        if (domainUser != null)
        {
            return domainUser.Id;
        }

        return await AddNewUser(authUserId, cancellationToken);
    }

    // TODO Подумать над переносом в сервис
    private async Task<int> AddNewUser(Guid authUserId, CancellationToken cancellationToken)
    {
        var domainUser = new DomainUser
        {
            AuthUserId = authUserId,
        };

        await context.DomainUsers.AddAsync(domainUser, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return domainUser.Id;
    }
}
