using Microsoft.AspNetCore.Identity;
using Money.Business;
using Money.Business.Services;
using Money.Data.Entities;
using OpenIddict.Abstractions;

namespace Money.Api.Middlewares;

public class AuthMiddleware(
    RequestDelegate next, 
    ILogger<ExceptionHandlingMiddleware> logger,
    RequestEnvironment environment,
    UserManager<ApplicationUser> userManager,
    AccountService accountService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(context.User.GetClaim(OpenIddictConstants.Claims.Subject) ?? string.Empty);
        if(user != null)
        {
            environment.UserId = accountService.GetOrCreateUserId(user.Id);
        }
        await next(context);
    }
}
