using Microsoft.AspNetCore.Identity;
using Money.Business;
using Money.Data.Entities;
using OpenIddict.Abstractions;

namespace Money.Api.Middlewares;

public class AuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        RequestEnvironment environment,
        UserManager<ApplicationUser> userManager,
        AccountService accountService)
    {
        var userId = context.User.GetClaim(OpenIddictConstants.Claims.Subject);

        if (userId != null)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user != null)
            {
                environment.UserId = await accountService.EnsureUserIdAsync(user.Id, context.RequestAborted);
            }
        }

        await next(context);
    }
}
