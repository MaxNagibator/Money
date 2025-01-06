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

    private CategorySelector? _categorySelector;
    private CategorySelector? _changeCategorySelector;
    private List<Operation>? _operations;

    private bool _showZeroDays;
    private bool _showDateRange = true;
    private bool _showChangeCategorySelector;

    public event EventHandler<OperationSearchEventArgs>? OnSearch;

    public DateRange DateRange { get; private set; } = new();

    [Inject]
    private ILocalStorageService StorageService { get; set; } = null!;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = null!;

    [Inject]
    private CategoryService CategoryService { get; set; } = null!;

    [Inject]
    private PlaceService PlaceService { get; set; } = null!;

    private string? Comment { get; set; }
    private string? Place { get; set; }
    private DateInterval? SelectedRange { get; set; }

    public async Task SearchAsync()
    {
        var categories = await CategoryService.GetAllAsync();

        var filter = new OperationClient.OperationFilterDto
        {
            CategoryIds = _categorySelector?.GetSelectedCategories(),
            Comment = Comment,
            Place = Place,
            DateFrom = DateRange.Start,
            DateTo = DateRange.End?.AddDays(1),
        };

        var apiOperations = await MoneyClient.Operation.Get(filter);

        if (apiOperations.Content == null)
        {
            return;
        }

        var categoriesDict = categories.ToDictionary(x => x.Id!.Value, x => x);

        var operations = apiOperations.Content
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

        OnSearch?.Invoke(this, new()
        {
            Operations = operations,
            AddZeroDays = _showZeroDays,
        });

        _operations = operations;
    }

    protected override async Task OnInitializedAsync()
    {
        var key = await StorageService.GetItemAsync<string?>(nameof(SelectedRange));
        var interval = DateIntervals.FirstOrDefault(interval => interval.DisplayName == key);
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

        DateRange = new(start, value.End.Invoke(start));
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

    private Task ResetAsync()
    {
        Comment = null;
        Place = null;
        _categorySelector?.Reset();
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

    private async Task TransferOperationsAsync()
    {
        if (_changeCategorySelector is not { SelectedCategory.Id: not null } || _operations == null)
        {
            return;
        }

        var request = new OperationClient.UpdateOperationsBatchRequest
        {
            OperationIds = _operations.Select(x => x.Id!.Value).ToList(),
            CategoryId = _changeCategorySelector.SelectedCategory.Id.Value,
        };

        await MoneyClient.Operation.UpdateBatch(request);
        _changeCategorySelector.Reset();
        _showChangeCategorySelector = false;

        await SearchAsync();
    }

    private async Task OnToggledChanged(bool toggled)
    {
        _showZeroDays = toggled;

        await Task.Run(() => OnSearch?.Invoke(this, new()
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
