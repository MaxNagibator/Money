using System.Security.Claims;
using OpenIddict.Abstractions;

namespace Money.Business.Interfaces;

public interface IAuthService
{
    public Task<ClaimsPrincipal> ExchangeAsync(OpenIddictRequest request);
}
