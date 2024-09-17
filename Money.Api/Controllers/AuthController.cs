using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Money.Business.Services;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Money.Api.Controllers;

public class AuthController(AuthService authorizationService) : Controller
{
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest()
                                    ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        ClaimsPrincipal claims = await authorizationService.ExchangeAsync(request);
        return SignIn(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
