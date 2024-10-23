using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Money.Web.Components;

public partial class DarkModeToggle : IDisposable
{
    private PeriodicTimer? _timer;
    private Task? _scheduledTask;

    [Parameter]
    public AppSettings AppSettings { get; set; } = default!;

    public DarkModeSettings Settings { get; set; } = default!;

    [Inject]
    private ILocalStorageService StorageService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

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

    public void Dispose()
    {
        _scheduledTask?.Dispose();
        _timer?.Dispose();
        NavigationManager.LocationChanged -= OnLocationChanged;

        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        Settings = await StorageService.GetItemAsync<DarkModeSettings>(nameof(DarkModeSettings)) ?? new DarkModeSettings();

        UpdateState();

        _timer = new PeriodicTimer(Settings.CheckInterval);
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

    private void UpdateState()
    {
        if (AppSettings is { IsManualMode: false, IsSchedule: false })
        {
            AppSettings.IsDarkMode = AppSettings.IsDarkModeSystem;

            if (_timer != null)
            {
                _timer.Period = new TimeSpan(24, 0, 0);
            }
        }

        if (AppSettings is { IsManualMode: false, IsSchedule: true })
        {
            CheckScheduledMode();
        }
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

        TimeSpan currentTime = DateTime.Now.TimeOfDay;

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
