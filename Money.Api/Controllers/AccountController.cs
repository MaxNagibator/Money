using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Common.Results;
using Money.Api.Services;
using Money.Api.ViewModels.Account;
using Money.BusinessLogic.Interfaces;
using Money.BusinessLogic.Models;

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
