using Refit;

namespace Money.Api.Constracts.Accounts
{
    public interface IAccountsResource
    {
        [Post("/Account/register")]
        Task RegisterAsync(
            [Body] AccountRegisterInfo model,
            CancellationToken cancellationToken = default);
    }
}