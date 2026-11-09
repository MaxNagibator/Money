using Microsoft.AspNetCore.Components;

namespace Money.Web.Common;

public static class NavigationManagerExtensions
{
    private static string SanitizeReturnUrl(NavigationManager navigationManager, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return navigationManager.BaseUri;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
        {
            var baseUri = new Uri(navigationManager.BaseUri);

            if (Uri.Compare(absolute, baseUri, UriComponents.SchemeAndServer, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return absolute.ToString();
            }

            return navigationManager.BaseUri;
        }

        if (url.StartsWith("//", StringComparison.Ordinal) || url.StartsWith('\\'))
        {
            return navigationManager.BaseUri;
        }

        return url;
    }

    extension(NavigationManager navigationManager)
    {
        public Uri GetUriWithReturnUrl(string url, string? returnUrl)
        {
            return new(navigationManager.GetUriWithQueryParameters(url, new Dictionary<string, object?> { ["ReturnUrl"] = returnUrl }), UriKind.Relative);
        }

        public void ReturnToSafe(string? url, bool forceLoad = false)
        {
            var target = SanitizeReturnUrl(navigationManager, url);
            navigationManager.NavigateTo(target, forceLoad);
        }
    }
}
