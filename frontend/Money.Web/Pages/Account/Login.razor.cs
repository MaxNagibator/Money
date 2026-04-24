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
    private AuthStateProvider AuthStateProvider { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    public async Task LoginUserAsync(EditContext context)
    {
        var user = new UserDto(Input.Login, Input.Password);

        var loginResult = await AuthenticationService.LoginAsync(user);

        if (loginResult.IsFailure)
        {
            Snackbar.Add($"Ошибка во время входа {loginResult.Error}", Severity.Error);
            return;
        }

        await AuthStateProvider.NotifyUserAuthentication();
        NavigationManager.ReturnToSafe(ReturnUrl ?? "operations");
    }

    protected override void OnParametersSet()
    {
        Input = new();
    }

    private void OnAuthLogin()
    {
        var url = AuthenticationService.GetExternalAuthUrl("auth", NavigationManager.BaseUri + "Account/Callback");
        NavigationManager.NavigateTo(url, true);
    }

    private void OnGitHubLogin()
    {
        var url = AuthenticationService.GetExternalAuthUrl("github", NavigationManager.BaseUri + "Account/Callback");
        NavigationManager.NavigateTo(url, true);
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
