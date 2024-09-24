using Microsoft.AspNetCore.Components.Routing;

namespace Money.Web.Layout;

public partial class NavMenu
{
    private string? _currentUrl;

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        _currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        _currentUrl = NavigationManager.ToBaseRelativePath(args.Location);
        StateHasChanged();
    }
}
