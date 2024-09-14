using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Money.BusinessLogic.Interfaces;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Money.Api.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authorizationService;

        public AuthController(IAuthService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpPost("~/connect/token")]
        [IgnoreAntiforgeryToken]
        [Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
            ClaimsPrincipal claims = await _authorizationService.ExchangeAsync(request);
            return SignIn(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
