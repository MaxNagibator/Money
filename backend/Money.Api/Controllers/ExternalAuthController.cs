using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Money.Data.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Client.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Money.Api.Controllers;

[ApiController]
[Route("external")]
public class ExternalAuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    AccountsService accountsService,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("login/auth")]
    public IActionResult LoginWithAuth([FromQuery] string? returnUrl = null)
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Content("~/connect/callback") };
        properties.SetString(OpenIddictClientAspNetCoreConstants.Properties.ProviderName, "Auth");

        var issuer = configuration["AUTH_AUTHORITY"];

        if (string.IsNullOrWhiteSpace(issuer) == false)
        {
            properties.SetString(OpenIddictClientAspNetCoreConstants.Properties.Issuer, issuer);
        }

        if (string.IsNullOrWhiteSpace(returnUrl) == false)
        {
            properties.Items["returnUrl"] = returnUrl;
        }

        return Challenge(properties, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("login/github")]
    public IActionResult LoginWithGitHub([FromQuery] string? returnUrl = null)
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Content("~/connect/callback") };
        properties.SetString(OpenIddictClientAspNetCoreConstants.Properties.ProviderName, "GitHub");

        if (string.IsNullOrWhiteSpace(returnUrl) == false)
        {
            properties.Items["returnUrl"] = returnUrl;
        }

        return Challenge(properties, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/connect/callback")]
    public async Task<IActionResult> Callback()
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
        var principal = result.Principal;

        if (principal == null)
        {
            return BadRequest("Не удалось аутентифицировать внешний провайдер.");
        }

        var userId = principal.FindFirst(OpenIddictConstants.Claims.Subject)?.Value
                     ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var email = principal.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal.FindFirst(OpenIddictConstants.Claims.Email)?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email обязателен для внешней аутентификации.");
        }

        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        var userNameCandidate = name ?? BuildValidUserName(email, userId);

        var providerName = result.Properties?.GetString(OpenIddictClientAspNetCoreConstants.Properties.ProviderName) ?? "GitHub";

        var user = userId != null ? await userManager.FindByLoginAsync(providerName, userId) : null;

        if (user == null)
        {
            user = await userManager.FindByEmailAsync(email);
        }

        if (user == null)
        {
            var uniqueUserName = userNameCandidate;
            var existing = await userManager.FindByNameAsync(uniqueUserName);

            if (existing != null)
            {
                uniqueUserName = $"{uniqueUserName}-{Guid.NewGuid().ToString("N")[..6]}";
            }

            user = new()
            {
                UserName = uniqueUserName,
                Email = email,
                EmailConfirmed = true,
            };

            var createResult = await userManager.CreateAsync(user);

            if (createResult.Succeeded == false)
            {
                return BadRequest(string.Join("; ", createResult.Errors.Select(e => e.Description)));
            }

            await accountsService.EnsureUserIdAsync(user.Id);
        }

        if (userId != null)
        {
            var logins = await userManager.GetLoginsAsync(user);

            if (logins.Any(x => x.LoginProvider == providerName && x.ProviderKey == userId) == false)
            {
                var addLoginResult = await userManager.AddLoginAsync(user, new(providerName, userId, providerName));

                if (addLoginResult.Succeeded == false)
                {
                    return BadRequest(string.Join("; ", addLoginResult.Errors.Select(e => e.Description)));
                }
            }
        }

        await signInManager.SignInAsync(user, true);

        var props = result.Properties ?? throw new InvalidOperationException("ReturnUrl отсутствует в контексте авторизации.");

        if (props.Items.TryGetValue("returnUrl", out var redirectUri) == false || string.IsNullOrWhiteSpace(redirectUri))
        {
            throw new InvalidOperationException("ReturnUrl отсутствует в контексте авторизации.");
        }

        return Redirect(redirectUri);
    }

    [HttpGet("denied")]
    public IActionResult AccessDenied()
    {
        return Forbid(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    // TODO: Переделать на нормальный запрос у пользователя
    private static string BuildValidUserName(string? email, string? subject)
    {
        string baseName;

        if (string.IsNullOrWhiteSpace(email) == false)
        {
            var at = email.IndexOf('@');
            baseName = at > 0 ? email[..at] : email;
        }
        else if (string.IsNullOrWhiteSpace(subject) == false)
        {
            baseName = $"gh_{subject}";
        }
        else
        {
            baseName = $"gh_{Guid.NewGuid().ToString("N")[..8]}";
        }

        var builder = new StringBuilder(baseName.Length);

        foreach (var ch in baseName)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
            else if (ch == '-' || ch == '_')
            {
                builder.Append(ch);
            }
            else if (char.IsWhiteSpace(ch))
            {
                builder.Append('-');
            }
        }

        var cleaned = Regex.Replace(builder.ToString(), "[-_]{2,}", "-").Trim('-', '_');

        if (string.IsNullOrEmpty(cleaned))
        {
            cleaned = $"gh_{Guid.NewGuid().ToString("N")[..8]}";
        }

        return cleaned;
    }
}
