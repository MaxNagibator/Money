using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Money.Web.Services.Authentication;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Pages.Account;

public partial class Register
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

    private InputType PasswordInputType => Input.UseConfirmPassword ? InputType.Password : InputType.Text;
    private string PasswordIcon => PasswordInputType == InputType.Password ? Icons.Material.Filled.VisibilityOff : Icons.Material.Filled.Visibility;

    public Task RegisterUserAsync(EditContext editContext)
    {
        var user = new RegisterUserDto(Input.UserName, Input.Email, Input.Password);

        return AuthenticationService.RegisterAsync(user)
            .Map(() => AuthenticationService.LoginAsync(new UserDto(Input.UserName, Input.Password)))
            .TapError(message => Snackbar.Add($"Ошибка во время регистрации {message}", Severity.Error))
            .Tap(() => NavigationManager.ReturnTo(ReturnUrl));
    }

    protected override void OnParametersSet()
    {
        Input = new();
    }

    private void TogglePasswordVisibility()
    {
        Input.UseConfirmPassword = !Input.UseConfirmPassword;
    }

    private sealed class InputModel : IValidatableObject
    {
        [Required(ErrorMessage = "Логин обязателен.")]
        [Display(Name = "Логин")]
        public string UserName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Некорректный email.")]
        [Display(Name = "Электронная почта")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool UseConfirmPassword { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UseConfirmPassword && Password != ConfirmPassword)
            {
                yield return new("Пароли не совпадают.", [nameof(ConfirmPassword)]);
            }
        }
    }
}
