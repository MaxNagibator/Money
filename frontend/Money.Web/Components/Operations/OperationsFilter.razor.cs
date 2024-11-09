using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Money.ApiClient;

namespace Money.Web.Components.Operations;

public partial class OperationsFilter
{
    private static readonly List<DateInterval> DateIntervals =
    [
        new("День", "ий день", time => time, time => time, (time, direction) => time.AddDays(direction)),
        new("Неделя", "ая неделя", time => time.StartOfWeek(), time => time.EndOfWeek(), (time, direction) => time.AddDays(7 * direction)),
        new("Месяц", "ий месяц", time => time.StartOfMonth(), time => time.EndOfMonth(), (time, direction) => time.AddMonths(direction)),
        new("Год", "ий год", time => time.StartOfYear(), time => time.EndOfYear(), (time, direction) => time.AddYears(direction)),
    ];

    private bool _isCategoriesTreeOpen;
    private bool _showZeroDays;
    private List<Operation>? _operations;

    public event EventHandler<OperationSearchEventArgs>? OnSearch;

    public DateRange DateRange { get; private set; } = new();

    [Inject]
    private ILocalStorageService StorageService { get; set; } = default!;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    [Inject]
    private PlaceService PlaceService { get; set; } = default!;

    private List<TreeItemData<Category>> InitialTreeItems { get; set; } = [];
    private IReadOnlyCollection<Category>? SelectedCategories { get; set; }

    private DateInterval? SelectedRange { get; set; }

    private string? Comment { get; set; }
    private string? Place { get; set; }
    private bool ShowDateRange { get; set; } = true;

    private List<Category>? Categories { get; set; }

    public async Task SearchAsync()
    {
        Categories ??= await CategoryService.GetCategories() ?? [];
        InitialTreeItems = Categories.BuildChildren(null).ToList();

        OperationClient.OperationFilterDto filter = new()
        {
            CategoryIds = SelectedCategories?.Select(x => x.Id!.Value).ToList(),
            Comment = Comment,
            Place = Place,
            DateFrom = DateRange.Start,
            DateTo = DateRange.End?.AddDays(1),
        };

        ApiClientResponse<OperationClient.Operation[]> apiOperations = await MoneyClient.Operation.Get(filter);

        if (apiOperations.Content == null)
        {
            return;
        }

        Dictionary<int, Category> categoriesDict = Categories!.ToDictionary(x => x.Id!.Value, x => x);

        List<Operation> operations = apiOperations.Content
            .Select(apiOperation => new Operation
            {
                Id = apiOperation.Id,
                Sum = apiOperation.Sum,
                Category = categoriesDict[apiOperation.CategoryId],
                Comment = apiOperation.Comment,
                Date = apiOperation.Date.Date,
                CreatedTaskId = apiOperation.CreatedTaskId,
                Place = apiOperation.Place,
            })
            .ToList();

        OnSearch?.Invoke(this, new OperationSearchEventArgs
        {
            Operations = operations,
            AddZeroDays = _showZeroDays,
        });

        _operations = operations;
    }

    protected override async Task OnInitializedAsync()
    {
        string? key = await StorageService.GetItemAsync<string?>(nameof(SelectedRange));
        DateInterval? interval = DateIntervals.FirstOrDefault(interval => interval.DisplayName == key);
        await OnDateIntervalChanged(interval);
        await SearchAsync();
    }

    private Task<IEnumerable<string?>> SearchPlaceAsync(string? value, CancellationToken token)
    {
        return PlaceService.SearchPlace(value, token)!;
    }

    private async Task OnDateIntervalChanged(DateInterval? value)
    {
        SelectedRange = value;

        if (value != null)
        {
            await UpdateDateRangeAsync(value);
            await StorageService.SetItemAsync(nameof(SelectedRange), SelectedRange?.DisplayName);
        }
    }

    private Task UpdateDateRangeAsync(DateInterval value)
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
        return SearchAsync();
    }

    private Task UpdateDateRangeAsync(Func<DateRange, DateRange> updateFunction)
    {
        if (SelectedRange == null)
        {
            return Task.CompletedTask;
        }

        DateRange = updateFunction.Invoke(DateRange);
        return SearchAsync();
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

    private Task ResetAsync()
    {
        Comment = null;
        Place = null;
        SelectedCategories = null;
        return SearchAsync();
    }

    private Task DecrementDateRangeAsync()
    {
        return UpdateDateRangeAsync(SelectedRange!.Decrement);
    }

    private Task IncrementDateRangeAsync()
    {
        return UpdateDateRangeAsync(SelectedRange!.Increment);
    }

    private void ToggleCategoriesTree(bool? isOpen = null)
    {
        isOpen ??= !_isCategoriesTreeOpen;
        _isCategoriesTreeOpen = isOpen.Value;
    }

    private async Task OnToggledChanged(bool toggled)
    {
        _showZeroDays = toggled;

        await Task.Run(() => OnSearch?.Invoke(this, new OperationSearchEventArgs
        {
            Operations = _operations,
            AddZeroDays = _showZeroDays,
            ShouldRender = false,
        }));

        StateHasChanged();
    }

    // Для проверки быстродействия
    /*
    private void OnToggledChanged(bool toggled)
    {
        _showZeroDays = toggled;

        OnSearch?.Invoke(this, new OperationSearchEventArgs
        {
            Operations = _operations,
            AddZeroDays = _showZeroDays,
            ShouldRender = false,
        });

        StateHasChanged();
    }*/
}
