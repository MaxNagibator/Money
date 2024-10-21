namespace Money.Web.Common;

public class AppSettings
{
    private bool _showDividers;
    private bool _isDarkMod;
    private bool _isDarkModeSystem;
    private bool _isManualMode;
    private bool _isSchedule;

    public event Action? OnChange;

    public bool ShowDividers
    {
        get => _showDividers;
        set
        {
            _showDividers = value;
            OnChange?.Invoke();
        }
    }

    public bool IsDarkMod
    {
        get => _isDarkMod;
        set
        {
            _isDarkMod = value;
            OnChange?.Invoke();
        }
    }

    public bool IsDarkModeSystem
    {
        get => _isDarkModeSystem;
        set
        {
            _isDarkModeSystem = value;
            OnChange?.Invoke();
        }
    }

    public bool IsManualMode
    {
        get => _isManualMode;
        set
        {
            _isManualMode = value;
            OnChange?.Invoke();
        }
    }

    public bool IsSchedule
    {
        get => _isSchedule;
        set
        {
            _isSchedule = value;
            OnChange?.Invoke();
        }
    }
}
