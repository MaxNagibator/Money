using OpenIddict.Abstractions;
using System.Security.Claims;

namespace Money.BusinessLogic.Interfaces
{
    public interface IAuthService
    {
        public Task<ClaimsPrincipal> ExchangeAsync(OpenIddictRequest request);
    }
}
