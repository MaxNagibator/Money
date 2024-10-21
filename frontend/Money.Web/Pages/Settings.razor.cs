using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace Money.Web.Pages;

public partial class Settings
{
    [CascadingParameter]
    public AppSettings AppSettings { get; set; } = default!;

    [Inject]
    private ILocalStorageService StorageService { get; set; } = default!;

    private TimeSpan? DarkModeStartTime { get; set; } = new(18, 0, 0);
    private TimeSpan? DarkModeEndTime { get; set; } = new(6, 0, 0);

    protected override async Task OnInitializedAsync()
    {
        DarkModeStartTime = await StorageService.GetItemAsync<TimeSpan?>(nameof(DarkModeStartTime)) ?? DarkModeStartTime;
        DarkModeEndTime = await StorageService.GetItemAsync<TimeSpan?>(nameof(DarkModeEndTime)) ?? DarkModeEndTime;

        if (AppSettings.IsSchedule)
        {
            CheckScheduledMode();
        }
    }

    private void ToggleDarkMode()
    {
        if (AppSettings.IsManualMode == false)
        {
            ToggleManualMode();
        }

        AppSettings.IsDarkMod = !AppSettings.IsDarkMod;
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
            bool systemPreference = AppSettings.IsDarkModeSystem;
            AppSettings.IsDarkMod = systemPreference;
        }
    }

    private async Task SetScheduledMode()
    {
        await StorageService.SetItemAsync(nameof(DarkModeStartTime), DarkModeStartTime);
        await StorageService.SetItemAsync(nameof(DarkModeEndTime), DarkModeEndTime);
        CheckScheduledMode();
    }

    private void CheckScheduledMode()
    {
        TimeSpan currentTime = DateTime.Now.TimeOfDay;

        if (DarkModeStartTime < DarkModeEndTime)
        {
            AppSettings.IsDarkMod = currentTime >= DarkModeStartTime && currentTime < DarkModeEndTime;
        }
        else
        {
            AppSettings.IsDarkMod = currentTime >= DarkModeStartTime || currentTime < DarkModeEndTime;
        }

        StateHasChanged();
    }
}
