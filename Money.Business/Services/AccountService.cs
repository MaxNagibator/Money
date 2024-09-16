using Microsoft.AspNetCore.Identity;
using Money.Business.Interfaces;
using Money.Business.Models;
using Money.Common.Exceptions;
using Money.Data;
using Money.Data.Entities;

namespace Money.Business.Services;

public class AccountService(UserManager<ApplicationUser> userManager, ApplicationDbContext context) : IAccountService
{
    public async Task RegisterAsync(RegisterViewModel model)
    {
        ApplicationUser? user = await userManager.FindByNameAsync(model.Email);

        if (user != null)
        {
            throw new EntityExistsException("Пользователь уже зарегистрирован.");
        }

        user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        IdentityResult result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded == false)
        {
            throw new Exception($"Ошибки: {string.Join("; ", result.Errors)}");
        }
        context.DomainUsers.Add(new DomainUser { AuthUserId = user.Id });
    }
    public int GetOrCreateUserId(Guid authUserId)
    {
        var domainUser = context.DomainUsers.FirstOrDefault(x => x.AuthUserId == authUserId);
        if (domainUser == null)
        {
            domainUser = new DomainUser { AuthUserId = authUserId };
            context.DomainUsers.Add(domainUser);
            context.SaveChanges();
        }
        return domainUser.Id;
    }
}
