using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components.Account.Pages;

public partial class Login
{
    private readonly string? _errorMessage = null;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; } = null;

    public async Task LoginUser()
    {
        await SignInManager.Login(new UserDto(Input.Email, Input.Password));

        // if (result.Succeeded)
        // {
        NavigationManager.NavigateTo(ReturnUrl ?? string.Empty);
        // }
        // else if (result.IsLockedOut)
        // {
        //     Logger.LogWarning("User account locked out.");
        //     RedirectManager.RedirectTo("Account/Lockout");
        // }
        // else
        // {
        //     errorMessage = "Error: Invalid login attempt.";
        // }
    }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
