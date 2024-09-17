using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Business.Models;
using Money.Business.Services;

namespace Money.Api.Controllers;

[Authorize]
[Route("[controller]")]
public class AccountController(AccountService accountService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }

        await accountService.RegisterAsync(model);
        return Ok();
    }
}
