using Microsoft.AspNetCore.Components;

namespace Money.Web.Components;

public partial class DarkModeToggle
{
    [Parameter]
    public AppSettings AppSettings { get; set; } = default!;

    protected override void OnInitialized()
    {
        if (AppSettings is { IsManualMode: false, IsSchedule: false })
        {
            AppSettings.IsDarkMode = AppSettings.IsDarkModeSystem;
        }
    }

    private void ToggleDarkMode()
    {
        if (AppSettings.IsManualMode == false)
        {
            ToggleManualMode();
        }

        AppSettings.IsDarkMode = !AppSettings.IsDarkMode;
    }

    private void ToggleManualMode()
    {
        AppSettings.IsManualMode = !AppSettings.IsManualMode;

        if (AppSettings.IsSchedule)
        {
            AppSettings.IsSchedule = false;
        }

        if (AppSettings.IsManualMode == false)
        {
            AppSettings.IsDarkMode = AppSettings.IsDarkModeSystem;
        }
    }
}
