using Money.ApiClient;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using Timer = System.Timers.Timer;

namespace Money.Web.Pages.Account;

public partial class Profile(
    AuthenticationStateProvider authenticationStateProvider,
    MoneyClient moneyClient,
    ISnackbar snackbar)
{
    private readonly InputModel _confirmationModel = new();
    private readonly ChangePasswordInputModel _changePaswordModel = new();

    private string _userName = "Anonymous";
    private string? _email;
    private bool _emailConfirmed;

    private bool _isProcessing;
    private bool _canResend = true;

    private Timer? _resendTimer;
    private int _remainingTime;

    protected override async Task OnInitializedAsync()
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        _userName = user.Identity!.Name!;
        _email = user.Identity.Name;
        _emailConfirmed = user.Claims.Any(x => x is { Type: "email_verified", Value: "true" });
    }

    private async Task SendConfirmationCode()
    {
        _isProcessing = true;

        var result = await moneyClient.Accounts.ResendConfirmCodeAsync();
        if (!result.GetError().ShowMessage(snackbar).HasError())
        {
            snackbar.Add("Новый код отправлен на вашу почту", Severity.Success);
        }

        _isProcessing = false;
    }

    private async Task HandleConfirmation()
    {
        _isProcessing = true;

        // TODO: Обработать ошибку
        var result = await moneyClient.Accounts.ConfirmEmailAsync(_confirmationModel.Code);

        if (result.IsSuccessStatusCode)
        {
            _emailConfirmed = true;
            _resendTimer?.Dispose();
            snackbar.Add("Почта успешно подтверждена!", Severity.Success);
            StateHasChanged();
        }

        _isProcessing = false;
    }

    private async Task HandleChangePassword()
    {
        _isProcessing = true;

        // TODO: Обработать ошибку
        var result = await moneyClient.Accounts.ChangePassword(_changePaswordModel.CurrentPassword, _changePaswordModel.NewPassword);

        if (result.IsSuccessStatusCode)
        {
            _changePaswordModel.CurrentPassword = "";
            _changePaswordModel.NewPassword = "";
            snackbar.Add("Пароль успешно поменян!", Severity.Success);
            StateHasChanged();
        }

        _isProcessing = false;
    }


    private Task ResendCode()
    {
        _canResend = false;
        _remainingTime = 30;
        StartResendTimer();
        return SendConfirmationCode();
    }

    private void StartResendTimer()
    {
        _resendTimer?.Dispose();
        _resendTimer = new(1000);

        _resendTimer.Elapsed += (_, _) =>
        {
            if (_remainingTime > 0)
            {
                _remainingTime--;
                InvokeAsync(StateHasChanged);
            }
            else
            {
                _canResend = true;
                _resendTimer.Stop();
            }
        };

        _resendTimer.Start();
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = "Заполни меня")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Код должен содержать 6 символов")]
        public string Code { get; set; } = string.Empty;
    }

    private sealed class ChangePasswordInputModel
    {
        [Required(ErrorMessage = "Заполни меня")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Заполни меня")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
