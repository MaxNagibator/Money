using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components.Account.Pages;

public partial class Register
{
    private readonly string? _identityErrors = null;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; } = null;

    private string? Message => _identityErrors is null ? null : _identityErrors;

    public async Task RegisterUser(EditContext editContext)
    {
        UserDto user = new(Input.Email, Input.Password);
        await SignInManager.Register(user);

        await SignInManager.Login(user);
        NavigationManager.NavigateTo(ReturnUrl ?? string.Empty);
    }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
