using Money.BusinessLogic.Models;

namespace Money.BusinessLogic.Interfaces
{
    public interface IAccountService
    {
        Task RegisterAsync(RegisterViewModel model);
    }
}
