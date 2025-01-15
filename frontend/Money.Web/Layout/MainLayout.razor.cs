using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace Money.Web.Layout;

public partial class MainLayout
{
    private readonly MudTheme _defaultTheme = new();
    private AppSettings _appSettings = new();

    private MudThemeProvider _mudThemeProvider = null!;
    private DarkModeToggle _darkModeToggle = null!;

    private bool _drawerOpen = true;

    [Inject]
    private ILocalStorageService StorageService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    private bool IsHomePage => string.Equals(NavigationManager.BaseUri, NavigationManager.Uri, StringComparison.OrdinalIgnoreCase);

    protected override async Task OnInitializedAsync()
    {
        _appSettings = await StorageService.GetItemAsync<AppSettings>(nameof(AppSettings)) ?? new AppSettings();

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

        _appSettings.IsDarkModeSystem = await _mudThemeProvider.GetSystemPreference();
        await _mudThemeProvider.WatchSystemPreference(OnSystemPreferenceChanged);
        _darkModeToggle.UpdateState();
        StateHasChanged();

        if (IsHomePage)
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity is { IsAuthenticated: true })
            {
                NavigationManager.NavigateTo("operations");
            }
        }
    }

    private async Task SaveSettings()
    {
        await StorageService.SetItemAsync(nameof(AppSettings), _appSettings);
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

        NavigationManager.NavigateTo("");
    }
}
