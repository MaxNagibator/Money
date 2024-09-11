using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Money.Api
{


    public class RegisterUser
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginUser
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class AuthenticationService2
    {
        UserManager<IdentityUser> _userManager;
        SignInManager<IdentityUser> _signInManager;

        public AuthenticationService2(UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<Tuple<bool, string[]>> RegisterNewUser(RegisterUser user)
        {
            var identityUser = new IdentityUser()
            {
                Email = user.Email,
                UserName = user.Email
            };
            var result = await _userManager.CreateAsync(identityUser, user.Password);
            if (result.Succeeded)
            {
                return new Tuple<bool, string[]>(true, null);
            }
            var errors = result.Errors.Select(x => x.Description).ToArray();
                return new Tuple<bool, string[]>(false, errors);
        }

        public async Task<bool> Authenticate(LoginUser user)
        {

            var result = await _signInManager.PasswordSignInAsync(user.Email, user.Password, false, true);

            if (result.Succeeded)
            {
                return true;
            }
            return false;
        }

    }
}
