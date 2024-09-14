using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Money.Data.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    public class InfoController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public InfoController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("I am Authorize");
        }

        [HttpGet("message")]
        public async Task<IActionResult> GetMessage()
        {
            ApplicationUser user = await _userManager.FindByIdAsync(User.GetClaim(OpenIddictConstants.Claims.Subject) ?? string.Empty);

            if (user == null)
            {
                return Challenge(authenticationSchemes: OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictValidationAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                        [OpenIddictValidationAspNetCoreConstants.Properties.ErrorDescription] =
                            "The specified access token is bound to an account that no longer exists."
                    }));
            }

            return Content($"{user.UserName} has been successfully authenticated.");
        }
    }
}
