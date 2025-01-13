using Microsoft.AspNetCore.Components;

namespace Money.Web.Common;

public static class NavigationManagerExtensions
{
    public static Uri GetUriWithReturnUrl(this NavigationManager navigationManager, string url, string? returnUrl)
    {
        return new(navigationManager.GetUriWithQueryParameters(url, new Dictionary<string, object?> { ["ReturnUrl"] = returnUrl }), UriKind.Relative);
    }

    public static void ReturnTo(this NavigationManager navigationManager, string? url, bool forceLoad = false)
    {
        navigationManager.NavigateTo(url ?? navigationManager.BaseUri, forceLoad);
    }
}
