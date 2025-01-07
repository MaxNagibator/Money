using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Money.Web.Layout;

public partial class NavMenu : IDisposable
{
    private string? _currentUrl;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

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
