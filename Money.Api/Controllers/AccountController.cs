using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Common.Results;
using Money.Api.Services;
using Money.Api.ViewModels.Account;

namespace Money.Api.Controllers;

[Authorize]
[Route("api/[controller]/[action]")]
public class AccountController(AccountService accountService) : Controller
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }

        Result result = await accountService.RegisterAsync(model);

        if (result.IsSuccess)
        {
            return Ok();
        }

        foreach (string error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return BadRequest(ModelState);
    }
}
