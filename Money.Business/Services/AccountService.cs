using Microsoft.AspNetCore.Identity;
using Money.Business.Interfaces;
using Money.Business.Models;
using Money.Common.Exceptions;
using Money.Data.Entities;

namespace Money.Business.Services;

public class AccountService(UserManager<ApplicationUser> userManager) : IAccountService
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
    }
}
