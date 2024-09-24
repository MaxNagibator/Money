using Microsoft.AspNetCore.Components;

namespace Money.Web.Components.Account.Pages;

public partial class Logout
{
    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; } = null;

    protected override async Task OnInitializedAsync()
    {
        await SignInManager.Logout();
        NavigationManager.NavigateTo(ReturnUrl ?? "", true);
    }
}
