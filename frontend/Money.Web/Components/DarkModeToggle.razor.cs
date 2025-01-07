using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Money.Web.Components;

public partial class DarkModeToggle : IDisposable
{
    private PeriodicTimer? _timer;
    private Task? _scheduledTask;

    [Parameter]
    public required AppSettings AppSettings { get; set; }

    public required DarkModeSettings Settings { get; set; }

    [Inject]
    private ILocalStorageService StorageService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    public async Task SetScheduledMode(TimeSpan? darkModeStartTime, TimeSpan? darkModeEndTime)
    {
        Settings.DarkStart = darkModeStartTime;
        Settings.DarkEnd = darkModeEndTime;

        await SaveAsync();

        CheckScheduledMode();
    }

    public void ToggleDarkMode()
    {
        if (AppSettings is { IsManualMode: false })
        {
            ToggleManualMode();
        }

        AppSettings.IsDarkMode = !AppSettings.IsDarkMode;
    }

    public void ToggleManualMode()
    {
        AppSettings.IsManualMode = !AppSettings.IsManualMode;

        UpdateState();
    }

    public void ToggleScheduleMode()
    {
        AppSettings.IsSchedule = !AppSettings.IsSchedule;

        UpdateState();
    }

    public async Task ToggleTimerEnabled()
    {
        Settings.IsTimerEnabled = !Settings.IsTimerEnabled;
        await SaveAsync();
    }

    public async Task ToggleLocationChanged()
    {
        Settings.IsLocationChangedEnabled = !Settings.IsLocationChangedEnabled;
        await SaveAsync();
    }

    public async Task SetInterval(TimeSpan? interval)
    {
        if (interval == null)
        {
            return;
        }

        Settings.CheckInterval = interval.Value;
        await SaveAsync();

        if (_timer != null)
        {
            _timer.Period = interval.Value;
        }
    }

    // TODO Придумать альтернативную синхронизацию компонентов
    public async Task<DarkModeSettings> GetSettingsAsync()
    {
        return await StorageService.GetItemAsync<DarkModeSettings>(nameof(DarkModeSettings)) ?? new DarkModeSettings();
    }

    public void Dispose()
    {
        _scheduledTask?.Dispose();
        _timer?.Dispose();
        NavigationManager.LocationChanged -= OnLocationChanged;

        GC.SuppressFinalize(this);
    }

    public void UpdateState()
    {
        if (AppSettings is { IsManualMode: false, IsSchedule: false })
        {
            AppSettings.IsDarkMode = AppSettings.IsDarkModeSystem;

            if (_timer != null)
            {
                _timer.Period = new(24, 0, 0);
            }
        }

        if (AppSettings is { IsManualMode: false, IsSchedule: true })
        {
            CheckScheduledMode();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        Settings = await GetSettingsAsync();

        UpdateState();

        _timer = new(Settings.CheckInterval);
        _scheduledTask = Task.Run(MonitorScheduledModeAsync);

        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (AppSettings is { IsManualMode: false } && Settings is { IsLocationChangedEnabled: true })
        {
            CheckScheduledMode();
        }
    }

    private async Task SaveAsync()
    {
        await StorageService.SetItemAsync(nameof(DarkModeSettings), Settings);
    }

    private async Task MonitorScheduledModeAsync()
    {
        while (await _timer!.WaitForNextTickAsync())
        {
            if (Settings.IsTimerEnabled)
            {
                CheckScheduledMode();
            }
        }
    }

    private void CheckScheduledMode()
    {
        if (AppSettings is { IsManualMode: true })
        {
            return;
        }

        var currentTime = DateTime.Now.TimeOfDay;

        if (Settings.DarkStart < Settings.DarkEnd)
        {
            AppSettings.IsDarkMode = currentTime >= Settings.DarkStart && currentTime < Settings.DarkEnd;
        }
        else
        {
            AppSettings.IsDarkMode = currentTime >= Settings.DarkStart || currentTime < Settings.DarkEnd;
        }

        StateHasChanged();
    }
}
