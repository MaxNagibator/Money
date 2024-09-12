using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Models;
using Money.Api.ViewModels.Account;

namespace Money.Api.Controllers;

[Authorize]
[Route("api/[controller]/[action]")]
public class AccountController(UserManager<ApplicationUser> userManager) : Controller
{
    // "email": "bob217@mail.ru",
    // "password": "222Aasdasdas123123123!"
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }

        ApplicationUser? user = await userManager.FindByNameAsync(model.Email);

        if (user != null)
        {
            return StatusCode(StatusCodes.Status409Conflict);
        }

        user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        IdentityResult result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return Ok();
        }

        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(ModelState);
    }
}
