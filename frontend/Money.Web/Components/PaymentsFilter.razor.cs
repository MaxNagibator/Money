using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Components;

public partial class PaymentsFilter
{
    private static readonly List<DateInterval> DateIntervals =
    [
        new("День", "ий день", time => time, time => time, (time, direction) => time.AddDays(direction)),
        new("Неделя", "ая неделя", time => time.StartOfWeek(), time => time.EndOfWeek(), (time, direction) => time.AddDays(7 * direction)),
        new("Месяц", "ий месяц", time => time.StartOfMonth(), time => time.EndOfMonth(), (time, direction) => time.AddMonths(direction)),
        new("Год", "ий год", time => time.StartOfYear(), time => time.EndOfYear(), (time, direction) => time.AddYears(direction)),
    ];

    private MudPopover _popover = null!;

    public event EventHandler<List<Payment>?>? OnSearch;

    public DateInterval? SelectedRange { get; set; }
    public List<Payment>? FilteredPayments { get; private set; }

    [Inject]
    private ILocalStorageService StorageService { get; set; } = default!;

    [Inject]
    private PlaceService PlaceService { get; set; } = default!;

    private List<TreeItemData<Category>> InitialTreeItems { get; set; } = [];
    private IReadOnlyCollection<Category>? SelectedCategories { get; set; }

    private string? Comment { get; set; }
    private string? Place { get; set; }

    private DateRange DateRange { get; set; } = new();
    private bool ShowDateRange { get; set; } = true;

    private List<Category>? Categories { get; set; }

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

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

    public async Task Search()
    {
        await GetCategories();
        UpdateCategories(Categories!);

        PaymentClient.PaymentFilterDto filter = GetFilter();
        ApiClientResponse<PaymentClient.Payment[]> apiPayments = await MoneyClient.Payment.Get(filter);

        if (apiPayments.Content == null)
        {
            return;
        }

        Dictionary<int, Category> categoriesDict = Categories!.ToDictionary(x => x.Id!.Value, x => x);

        List<Payment> payments = apiPayments.Content
            .Select(apiPayment => new Payment
            {
                Id = apiPayment.Id,
                Sum = apiPayment.Sum,
                Category = categoriesDict[apiPayment.CategoryId],
                Comment = apiPayment.Comment,
                Date = apiPayment.Date.Date,
                CreatedTaskId = apiPayment.CreatedTaskId,
                Place = apiPayment.Place,
            })
            .ToList();

        FilteredPayments = payments;
        OnSearch?.Invoke(this, payments);
    }

    protected override async Task OnInitializedAsync()
    {
        string? key = await StorageService.GetItemAsync<string?>(nameof(SelectedRange));
        DateInterval? interval = DateIntervals.FirstOrDefault(interval => interval.DisplayName == key);
        await OnDateIntervalChanged(interval);
        await Search();
    }

    private async Task<IEnumerable<string>> SearchPlace(string value, CancellationToken token)
    {
        return await PlaceService.SearchPlace(value, token);
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
        await Search();
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

    private async Task Reset()
    {
        Comment = null;
        Place = null;
        SelectedCategories = null;
        await Search();
    }

    private async Task UpdateDateRange(Func<DateRange, DateRange> updateFunction)
    {
        if (SelectedRange == null)
        {
            return;
        }

        DateRange = updateFunction.Invoke(DateRange);
        await Search();
    }

    private async Task DecrementDateRange()
    {
        await UpdateDateRange(SelectedRange!.Decrement);
    }

    private async Task IncrementDateRange()
    {
        await UpdateDateRange(SelectedRange!.Increment);
    }

    private async Task GetCategories()
    {
        Categories ??= await CategoryService.GetCategories();
    }
}
