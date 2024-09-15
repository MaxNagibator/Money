using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Business.Interfaces;
using Money.Business.Models;

namespace Money.Api.Controllers;

[Authorize]
[Route("[controller]")]
public class AccountController(IAccountService accountService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }

        await accountService.RegisterAsync(model);
        return Ok();
    }
}
