using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Components;

public partial class PaymentsFilter
{
    private List<DateInterval> _dateIntervals =
    [
        new("День", "ий день", time => time, time => time, (time, direction) => time.AddDays(direction)),
        new("Неделя", "ая неделя", time => time.StartOfWeek(), time => time.EndOfWeek(), (time, direction) => time.AddDays(7 * direction)),
        new("Месяц", "ий месяц", time => time.StartOfMonth(), time => time.EndOfMonth(), (time, direction) => time.AddMonths(direction)),
        new("Год", "ий год", time => time.StartOfYear(), time => time.EndOfYear(), (time, direction) => time.AddYears(direction)),
    ];

    private MudPopover _popover = null!;
    private MudTextField<string> _comment = null!;

    [Parameter]
    public EventCallback<PaymentClient.PaymentFilterDto> OnSearch { get; set; }

    public DateInterval? SelectedRange { get; set; }

    [Inject]
    private ILocalStorageService StorageService { get; set; } = default!;

    private List<TreeItemData<Category>> InitialTreeItems { get; set; } = [];
    private IReadOnlyCollection<Category>? SelectedCategories { get; set; }

    private string? Comment { get; set; }
    private string? Place { get; set; }

    private DateRange DateRange { get; set; } = new();
    private bool ShowDateRange { get; set; } = true;

    public PaymentClient.PaymentFilterDto GetFilter()
    {
        return new PaymentClient.PaymentFilterDto
        {
            CategoryIds = SelectedCategories?.Select(x => x.Id!.Value).ToList(),
            Comment = Comment,
            Place = Place,
            DateFrom = DateRange.Start,
            DateTo = DateRange.End?.AddDays(1),
        };
    }

    public void UpdateCategories(List<Category> categories)
    {
        InitialTreeItems = categories.BuildChildren(null).ToList();
    }

    protected override async Task OnInitializedAsync()
    {
        string? key = await StorageService.GetItemAsync<string?>(nameof(SelectedRange));
        DateInterval? interval = _dateIntervals.FirstOrDefault(interval => interval.DisplayName == key);
        await OnDateIntervalChanged(interval);
    }

    private async Task OnDateIntervalChanged(DateInterval? value)
    {
        SelectedRange = value;

        if (value != null)
        {
            await UpdateDateRange(value);
            await StorageService.SetItemAsync(nameof(SelectedRange), SelectedRange?.DisplayName);
        }
    }

    private async Task UpdateDateRange(DateInterval value)
    {
        DateTime start;

        if (DateRange.Start != null)
        {
            start = value.Start.Invoke(DateRange.Start.Value);
        }
        else if (DateRange.End != null)
        {
            start = value.Start.Invoke(DateRange.End.Value);
        }
        else
        {
            start = value.Start.Invoke(DateTime.Today);
        }

        DateRange = new DateRange(start, value.End.Invoke(start));
        await OnSearch.InvokeAsync(GetFilter());
    }

    private string GetHelperText()
    {
        if (SelectedCategories == null || SelectedCategories.Count == 0)
        {
            return "Выберите категории";
        }

        return string.Join(", ", SelectedCategories.Select(x => x.Name));
    }

    private void OnTextChanged(string searchTerm)
    {
        InitialTreeItems.Filter(searchTerm);
    }

    private async Task DecrementDateRange()
    {
        if (SelectedRange == null)
        {
            return;
        }

        DateRange = SelectedRange.Decrement(DateRange);
        await OnSearch.InvokeAsync(GetFilter());
    }

    private async Task IncrementDateRange()
    {
        if (SelectedRange == null)
        {
            return;
        }

        DateRange = SelectedRange.Increment(DateRange);
        await OnSearch.InvokeAsync(GetFilter());
    }
}
