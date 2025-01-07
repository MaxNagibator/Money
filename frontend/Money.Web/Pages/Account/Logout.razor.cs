using Microsoft.AspNetCore.Components;
using Money.Web.Services.Authentication;

namespace Money.Web.Pages.Account;

public partial class Logout
{
    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [Inject]
    private AuthenticationService AuthenticationService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await AuthenticationService.LogoutAsync();
        NavigationManager.ReturnTo(ReturnUrl, true);
    }
}
