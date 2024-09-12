using Microsoft.AspNetCore.Identity;
using Money.Api.Common;
using Money.Api.Common.Extensions;
using Money.Api.Data;
using Money.Api.ViewModels.Account;

namespace Money.Api.Services;

public class AccountService(UserManager<ApplicationUser> userManager)
{
    public async Task<Result> RegisterAsync(RegisterViewModel model)
    {
        ApplicationUser? user = await userManager.FindByNameAsync(model.Email);

        if (user != null)
        {
            return Result.Failure("Пользователь уже существует");
        }

        user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        IdentityResult result = await userManager.CreateAsync(user, model.Password);

        return result.ToResult();
    }
}
