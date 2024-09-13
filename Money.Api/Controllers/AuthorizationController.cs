using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Common;
using Money.Api.Services;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Money.Api.Controllers;

public class AuthorizationController(AuthorizationService authorizationService) : Controller
{
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        Result<ClaimsPrincipal> result = await authorizationService.ExchangeAsync(request);

        if (result.IsSuccess)
        {
            return SignIn(result.Data!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        AuthenticationProperties properties = new(new Dictionary<string, string?>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = string.Join(", ", result.Errors)
        });

        return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
