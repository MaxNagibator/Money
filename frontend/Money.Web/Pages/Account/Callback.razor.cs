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
    private AuthStateProvider AuthStateProvider { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var result = await AuthenticationService.ExchangeExternalAsync();

        if (result.IsFailure)
        {
            Snackbar.Add($"Ошибка во время входа {result.Error}", Severity.Error);
            return;
        }

        await AuthStateProvider.NotifyUserAuthentication();
        NavigationManager.ReturnToSafe(ReturnUrl ?? "operations");
    }
}
