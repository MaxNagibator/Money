using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace Money.Web.Layout;

public partial class MainLayout
{
    private readonly MudTheme _defaultTheme = new()
    {
        PaletteLight = new PaletteLight
        {
            Black = "#272c34",
            HoverOpacity = 0.10,
            TableHover = "#66a1fa4d",
            Background = "#f5f5f5",
            DrawerBackground = "#f5f5f5",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#776be7",
            PrimaryContrastText = "rgba(255,255,255, 0.90)",
            PrimaryDarken = "rgb(90,71,255)",
            PrimaryLighten = "rgb(163,153,255)",
            HoverOpacity = 0.10,
            TableHover = "#66a1fa4d",
            Black = "#1a1a27",
            Background = "#1a1a27ff",
            BackgroundGray = "#151521ff",
            Surface = "#1e1e2dff",
            DrawerBackground = "#1a1a27ff",
            DrawerText = "rgba(255,255,255, 0.50)",
            DrawerIcon = "rgba(255,255,255, 0.50)",
            AppbarBackground = "#1a1a27cc",
            AppbarText = "rgba(255,255,255, 0.70)",
            TextPrimary = "rgba(255,255,255, 0.70)",
            TextSecondary = "rgba(255,255,255, 0.50)",
            ActionDefault = "#74718eff",
            ActionDisabled = "#9999994d",
            ActionDisabledBackground = "#605f6d4d",
            Divider = "#292838ff",
            DividerLight = "#000000cc",
            TableLines = "#33323eff",
            TableStriped = "#00000005",
            LinesDefault = "#33323eff",
            LinesInputs = "#bdbdbdff",
            TextDisabled = "rgba(255,255,255, 0.2)",
            Info = "#4a86ffff",
            Success = "#3dcb6cff",
            Warning = "#ffb545ff",
            Error = "#ff3f5fff",
            Dark = "#424242ff",
        },
    };

    private AppSettings _appSettings = new();

    private MudThemeProvider _mudThemeProvider = default!;

    [Inject]
    private ILocalStorageService StorageService { get; set; } = default!;

    private bool DrawerOpen { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        _appSettings = await StorageService.GetItemAsync<AppSettings>(nameof(AppSettings)) ?? new AppSettings();
        _appSettings.OnChange += async () => await SaveSettings();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
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
