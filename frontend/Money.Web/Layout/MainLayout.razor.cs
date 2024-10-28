using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Money.Web.Components;

namespace Money.Web.Layout;

public partial class MainLayout
{
    private readonly MudTheme _defaultTheme = new();
    private AppSettings _appSettings = new();

    private MudThemeProvider _mudThemeProvider = default!;
    private DarkModeToggle _darkModeToggle = default!;

    [Inject]
    private ILocalStorageService StorageService { get; set; } = default!;

    private bool DrawerOpen { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        _appSettings = await StorageService.GetItemAsync<AppSettings>(nameof(AppSettings)) ?? new AppSettings();

        _appSettings.OnChange += async () =>
        {
            await SaveSettings();
            StateHasChanged();
        };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _appSettings.IsDarkModeSystem = await _mudThemeProvider.GetSystemPreference();
            await _mudThemeProvider.WatchSystemPreference(OnSystemPreferenceChanged);

            StateHasChanged();
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
        DrawerOpen = !DrawerOpen;
    }
}
