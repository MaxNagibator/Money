using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.BusinessLogic.Models;
using Money.BusinessLogic.Interfaces;

namespace Money.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            await _accountService.RegisterAsync(model);
            return Ok();
        }
    }
}
