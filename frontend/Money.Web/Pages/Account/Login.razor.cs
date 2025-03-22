using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Money.Web.Services.Authentication;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Pages.Account;

public partial class Login
{
    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = null!;

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [Inject]
    private AuthenticationService AuthenticationService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    public Task LoginUserAsync(EditContext context)
    {
        var user = new UserDto(Input.Login, Input.Password);

        return AuthenticationService.LoginAsync(user)
            .TapError(message => Snackbar.Add($"Ошибка во время входа {message}", Severity.Error))
            .Map(() => ReturnUrl ?? "operations")
            .Tap(x => NavigationManager.ReturnTo(x));
    }

    protected override void OnParametersSet()
    {
        Input = new();
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = "Login обязателен.")]
        [Display(Name = "Логин или Email")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;
    }
}
