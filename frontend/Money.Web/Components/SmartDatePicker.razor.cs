using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Linq.Expressions;

namespace Money.Web.Components;

public partial class SmartDatePicker : ComponentBase
{
    private bool _isDatePickerVisible = true;
    private string? _dateText = string.Empty;

    [Parameter]
    public DateTime? Date { get; set; }

    [Parameter]
    public EventCallback<DateTime?> DateChanged { get; set; }

    [Parameter]
    public Func<DateTime?>? GetInitialDate { get; set; }

    [Parameter]
    public Expression<Func<DateTime?>>? For { get; set; }

    [Inject]
    private ILogger<SmartDatePicker> Logger { get; set; } = null!;

    public async Task<DateTime?> GetDateAsync()
    {
        if (await TryUpdateDateAsync())
        {
            return Date;
        }

        return null;
    }

    protected override async Task OnInitializedAsync()
    {
        if (GetInitialDate != null)
        {
            await UpdateDateAsync(GetInitialDate.Invoke());
        }
    }

    private async Task<bool> TryUpdateDateAsync()
    {
        if (_isDatePickerVisible)
        {
            return true;
        }

        DateTime? date = null;

        if (_dateText != null)
        {
            date = ParseDate();

            if (date == null)
            {
                return false;
            }
        }

        Date = date;
        _dateText = Date?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);
        await DateChanged.InvokeAsync(Date);

        return true;
    }

    private async Task UpdateDateAsync(DateTime? date)
    {
        Date = date;
        _dateText = date?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);
        await DateChanged.InvokeAsync(Date);
    }

    private async Task ToggleDateFieldAsync()
    {
        if (_isDatePickerVisible)
        {
            _dateText = Date?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);
        }
        else if (await TryUpdateDateAsync() == false)
        {
            return;
        }

        _isDatePickerVisible = !_isDatePickerVisible;
        await Task.Delay(10);
        StateHasChanged();
    }

    private DateTime? ParseDate()
    {
        DateTime? date = null;

        if (string.IsNullOrWhiteSpace(_dateText))
        {
            return date;
        }

        try
        {
            var normalizedText = _dateText.Trim().ToLowerInvariant();

            date = normalizedText switch
            {
                "сегодня" => DateTime.Now.Date,
                "завтра" => DateTime.Now.Date.AddDays(1),
                "вчера" => DateTime.Now.Date.AddDays(-1),
                _ => null,
            };

            if (date == null && DateTime.TryParseExact(_dateText, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedDate))
            {
                date = parsedDate;
            }
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Ошибка при разборе даты. Входные данные: {DateText}", _dateText);
        }

        return date;
    }
}
