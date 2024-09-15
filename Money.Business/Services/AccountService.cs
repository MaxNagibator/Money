using Money.Common.Exceptions;
using Microsoft.AspNetCore.Identity;
using Money.BusinessLogic.Interfaces;
using Money.Data.Entities;
using Money.BusinessLogic.Models;

namespace Money.BusinessLogic.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task RegisterAsync(RegisterViewModel model)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(model.Email);
            if (user != null)
            {
                throw new EntityExistsException("Пользователь уже зарегистрирован.");
            }

            user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                throw new Exception($"Ошибки: {string.Join("; ", result.Errors)}");
            }
        }
    }
}
