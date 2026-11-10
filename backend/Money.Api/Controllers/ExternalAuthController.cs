using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Money.Api.Definitions;
using Money.Data.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Client.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Money.Api.Controllers;

[ApiController]
[EnableRateLimiting(AuthorizationDefinition.SensitiveAuthPolicy)]
[Route("external")]
public class ExternalAuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    AccountsService accountsService,
    IConfiguration configuration,
    IHostEnvironment environment) : ControllerBase
{
    [HttpGet("login/auth")]
    public IActionResult LoginWithAuth([FromQuery] string? returnUrl = null)
    {
        if (!IsAllowedReturnUrl(returnUrl))
        {
            return BadRequest("Недопустимый адрес возврата.");
        }

        var properties = new AuthenticationProperties { RedirectUri = Url.Content("~/connect/callback") };
        properties.SetString(OpenIddictClientAspNetCoreConstants.Properties.ProviderName, "Auth");

        var issuer = configuration["AUTH_AUTHORITY"];

        if (!string.IsNullOrWhiteSpace(issuer))
        {
            properties.SetString(OpenIddictClientAspNetCoreConstants.Properties.Issuer, issuer);
        }

        properties.Items["returnUrl"] = returnUrl!;

        return Challenge(properties, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("login/github")]
    public IActionResult LoginWithGitHub([FromQuery] string? returnUrl = null)
    {
        if (!IsAllowedReturnUrl(returnUrl))
        {
            return BadRequest("Недопустимый адрес возврата.");
        }

        var properties = new AuthenticationProperties { RedirectUri = Url.Content("~/connect/callback") };
        properties.SetString(OpenIddictClientAspNetCoreConstants.Properties.ProviderName, "GitHub");

        properties.Items["returnUrl"] = returnUrl!;

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

        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("Не удалось определить идентификатор пользователя внешнего провайдера.");
        }

        var email = principal.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal.FindFirst(OpenIddictConstants.Claims.Email)?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email обязателен для внешней аутентификации.");
        }

        var isExternalEmailVerified = IsExternalEmailVerified(principal);

        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        var userNameCandidate = name ?? BuildValidUserName(email, userId);

        var providerName = result.Properties?.GetString(OpenIddictClientAspNetCoreConstants.Properties.ProviderName) ?? "GitHub";

        var user = await userManager.FindByLoginAsync(providerName, userId);

        if (user == null)
        {
            var existingByEmail = await userManager.FindByEmailAsync(email);

            if (existingByEmail != null)
            {
                if (!isExternalEmailVerified || !existingByEmail.EmailConfirmed)
                {
                    return BadRequest("Автоматическая привязка внешнего входа возможна только при подтвержденном email.");
                }

                user = existingByEmail;
            }
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
                EmailConfirmed = isExternalEmailVerified,
            };

            var createResult = await userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                return BadRequest(string.Join("; ", createResult.Errors.Select(e => e.Description)));
            }

            await accountsService.EnsureUserIdAsync(user.Id);
        }

        var logins = await userManager.GetLoginsAsync(user);

        if (!logins.Any(x => x.LoginProvider == providerName && x.ProviderKey == userId))
        {
            var addLoginResult = await userManager.AddLoginAsync(user, new(providerName, userId, providerName));

            if (!addLoginResult.Succeeded)
            {
                return BadRequest(string.Join("; ", addLoginResult.Errors.Select(e => e.Description)));
            }
        }

        await signInManager.SignInAsync(user, true);

        var props = result.Properties ?? throw new InvalidOperationException("ReturnUrl отсутствует в контексте авторизации.");

        if (!props.Items.TryGetValue("returnUrl", out var redirectUri)
            || !IsAllowedReturnUrl(redirectUri))
        {
            return BadRequest("Недопустимый адрес возврата.");
        }

        return Redirect(redirectUri!);
    }

    [HttpGet("denied")]
    public IActionResult AccessDenied()
    {
        return Forbid(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    private static bool IsExternalEmailVerified(ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(OpenIddictConstants.Claims.EmailVerified)?.Value;
        return bool.TryParse(claim, out var verified) && verified;
    }

    private bool IsAllowedReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return false;
        }

        if (Url.IsLocalUrl(returnUrl))
        {
            return true;
        }

        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var targetUri))
        {
            return false;
        }

        if (targetUri.Scheme != Uri.UriSchemeHttp && targetUri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        var configuredOrigins = (configuration["CORS_ORIGIN"] ?? "https://localhost:7168")
            .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var rawOrigin in configuredOrigins)
        {
            if (Uri.TryCreate(rawOrigin, UriKind.Absolute, out var allowedUri)
                && Uri.Compare(targetUri,
                    allowedUri,
                    UriComponents.SchemeAndServer,
                    UriFormat.Unescaped,
                    StringComparison.OrdinalIgnoreCase)
                == 0)
            {
                return true;
            }
        }

        return environment.IsDevelopment()
               && (string.Equals(targetUri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(targetUri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase));
    }

    // TODO: Переделать на нормальный запрос у пользователя
    private static string BuildValidUserName(string? email, string? subject)
    {
        string baseName;

        if (!string.IsNullOrWhiteSpace(email))
        {
            var at = email.IndexOf('@');
            baseName = at > 0 ? email[..at] : email;
        }
        else if (!string.IsNullOrWhiteSpace(subject))
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
