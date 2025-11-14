using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Money.Web.Layout;

public partial class MainLayout(
    IWebAssemblyHostEnvironment environment,
    ILocalStorageService storageService,
    NavigationManager navigationManager,
    AuthenticationStateProvider authenticationStateProvider)
{
    private readonly MudTheme _defaultTheme = new();
    private AppSettings _appSettings = new();

    private MudThemeProvider _mudThemeProvider = null!;
    private DarkModeToggle _darkModeToggle = null!;

    private bool _drawerOpen = true;

    private bool IsHomePage => string.Equals(navigationManager.BaseUri, navigationManager.Uri, StringComparison.OrdinalIgnoreCase);

    protected override async Task OnInitializedAsync()
    {
        _appSettings = await storageService.GetItemAsync<AppSettings>(nameof(AppSettings)) ?? new AppSettings();

        _appSettings.OnChange += async (_, _) =>
        {
            await SaveSettings();
            StateHasChanged();
        };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender == false)
        {
            return;
        }

        _appSettings.IsDarkModeSystem = await _mudThemeProvider.GetSystemDarkModeAsync();
        await _mudThemeProvider.WatchSystemDarkModeAsync(OnSystemPreferenceChanged);
        _darkModeToggle.UpdateState();
        StateHasChanged();

        if (IsHomePage)
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity is { IsAuthenticated: true })
            {
                navigationManager.NavigateTo("operations");
            }
        }
    }

    private async Task SaveSettings()
    {
        await storageService.SetItemAsync(nameof(AppSettings), _appSettings);
        StateHasChanged();
    }

    private Task OnSystemPreferenceChanged(bool newValue)
    {
        _appSettings.IsDarkModeSystem = newValue;
        StateHasChanged();

        return Task.CompletedTask;
    }

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }

    private void NavigateToHome()
    {
        if (IsHomePage)
        {
            return;
        }

        navigationManager.NavigateTo("");
    }
}
