namespace Money.Web.Common;

public sealed class AppSettings
{
    public event EventHandler? OnChange;

    public bool ShowDividers
    {
        get;
        set => SetValue(ref field, value);
    }

    public AlternateRowStyle AlternateRowStyle
    {
        get;
        set => SetValue(ref field, value);
    }

    public bool AlignMoneyDecimals
    {
        get;
        set => SetValue(ref field, value);
    } = true;

    public int MoneyColumnWidth
    {
        get;
        set => SetValue(ref field, Math.Clamp(value, 4, 14));
    } = 9;

    public bool UseChartThemeColors
    {
        get;
        set => SetValue(ref field, value);
    } = true;

    // TODO: Переделать на состояния
    public bool IsDarkMode
    {
        get;
        set => SetValue(ref field, value);
    }

    public bool IsDarkModeSystem
    {
        get;
        set => SetValue(ref field, value);
    }

    public bool IsManualMode
    {
        get;
        set => SetValue(ref field, value);
    }

    public bool IsSchedule
    {
        get;
        set => SetValue(ref field, value);
    }

    private void SetValue<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnChange?.Invoke(this, EventArgs.Empty);
    }
}
