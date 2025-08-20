using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Components;
using Money.Web.Services.Authentication;

namespace Money.Web.Pages.Account;

public partial class Callback
{
    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [Inject]
    private AuthenticationService AuthenticationService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    protected override Task OnInitializedAsync()
    {
        return AuthenticationService.ExchangeExternalAsync()
            .TapError(message => Snackbar.Add($"Ошибка во время входа {message}", Severity.Error))
            .Map(() => ReturnUrl ?? "operations")
            .Tap(x => NavigationManager.ReturnTo(x));
    }
}
