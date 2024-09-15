using Money.Business.Models;

namespace Money.Business.Interfaces;

public interface IAccountService
{
    Task RegisterAsync(RegisterViewModel model);
}
