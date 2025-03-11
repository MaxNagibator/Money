using Microsoft.AspNetCore.Components;
using Money.ApiClient;
using System.ComponentModel.DataAnnotations;
using Timer = System.Timers.Timer;

namespace Money.Web.Pages.Account;

public partial class Auth : IDisposable
{
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    private bool _emailConfirmed;
    private string _email;
    private readonly InputModel _confirmationModel = new();
    private string _errorMessage;
    private bool _isProcessing;
    private bool _canResend = true;
    private int _remainingTime;
    private Timer? _resendTimer;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        _email = user.Identity.Name;
        _emailConfirmed = user.Claims.Any(x => x is { Type: "email_verified", Value: "true" });
    }

    private async Task SendConfirmationCode()
    {
        try
        {
            _isProcessing = true;
            await MoneyClient.Accounts.ResendConfirmCodeAsync();
            Snackbar.Add("Код подтверждения отправлен на вашу почту", Severity.Success);
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task HandleConfirmation()
    {
        try
        {
            _isProcessing = true;
            var result = await MoneyClient.Accounts.ConfirmEmailAsync(_confirmationModel.Code);

            if (result.IsSuccessStatusCode)
            {
                _emailConfirmed = true;
                _resendTimer?.Dispose();
                Snackbar.Add("Почта успешно подтверждена!", Severity.Success);
            }
            else
            {
                _errorMessage = result.GetError()!.Title;
            }
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task ResendCode()
    {
        _canResend = false;
        _remainingTime = 30;
        StartResendTimer();
        await SendConfirmationCode();
    }

    private void StartResendTimer()
    {
        _resendTimer?.Dispose();
        _resendTimer = new(1000);

        _resendTimer.Elapsed += (sender, e) =>
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

    public void Dispose()
    {
        _resendTimer?.Dispose();
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = "Заполни меня")]
        public string Code { get; set; } = string.Empty;
    }
}
