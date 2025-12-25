using Microsoft.AspNetCore.Components;
using Money.Web.Models.Charts;

namespace Money.Web.Pages.Operations;

public sealed partial class Statistics : OperationComponent
{
    private Dictionary<int, OperationTypeStatistics>? _typesStatistics;
    private List<Category>? _categories;

    [CascadingParameter]
    public AppSettings AppSettings { get; set; } = null!;

    [Inject]
    private CategoryService CategoryService { get; set; } = null!;

    protected override void Dispose(bool disposing)
    {
        AppSettings.OnChange -= OnSettingsChanged;
        base.Dispose(disposing);
    }

    protected override async Task OnInitializedAsync()
    {
        AppSettings.OnChange += OnSettingsChanged;

        Dictionary<int, OperationTypeStatistics> typesStatistics = [];

        foreach (var operationType in OperationTypes.Values.Select(x => x.Id))
        {
            var barChart = BarChart.Create(operationType, AppSettings.UseChartThemeColors);
            var pieChart = PieChart.Create(operationType, AppSettings.UseChartThemeColors);
            typesStatistics.Add(operationType, new(barChart, pieChart));
        }

        _typesStatistics = typesStatistics;
        _categories = await CategoryService.GetAllAsync();
    }

    protected override void OnSearchChanged(object? sender, OperationSearchEventArgs args)
    {
        List<Task> tasks = [];

        var operationGroups = args.Operations?
            .GroupBy(x => x.Category.Id!.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());

        foreach (var operationTypeId in OperationTypes.Values.Select(x => x.Id))
        {
            var categories = args.Operations?
                                 .Where(x => x.Category.OperationType.Id == operationTypeId)
                                 .Select(x => x.Category)
                                 .DistinctBy(x => x.Id)
                                 .ToList()
                             ?? [];

            tasks.Add(_typesStatistics![operationTypeId].BarChart.UpdateAsync(args.Operations, categories, OperationsFilter.DateRange).AsTask());

            if (_categories is not { Count: not 0 } || operationGroups == null)
            {
                continue;
            }

            var cats = _categories.Where(x => x.OperationType.Id == operationTypeId).ToList();
            var categorySums = CalculateCategorySums(cats, operationGroups, null);

            tasks.Add(_typesStatistics![operationTypeId].PieChart.UpdateAsync(categorySums).AsTask());
            var sums = BuildChildren(categorySums);

            _typesStatistics[operationTypeId].Sums =
            [
                new()
                {
                    Value = new()
                    {
                        Name = "Всего",
                        TotalSum = sums.Sum(x => x.Value?.TotalSum ?? 0),
                    },
                    Children = sums,
                },
            ];
        }

        _ = Task.WhenAll(tasks);
        StateHasChanged();
    }

    private async void OnSettingsChanged(object? sender, EventArgs e)
    {
        if (_typesStatistics == null)
        {
            return;
        }

        foreach (var statistics in _typesStatistics.Values)
        {
            statistics.BarChart.UpdateTheme(AppSettings.UseChartThemeColors);
            statistics.PieChart.UpdateTheme(AppSettings.UseChartThemeColors);

            if (statistics.BarChart.Chart != null)
            {
                await statistics.BarChart.Chart.UpdateAsync();
            }

            if (statistics.PieChart.Chart != null)
            {
                await statistics.PieChart.Chart.UpdateAsync();
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    private List<TreeItemData<OperationCategorySum>> BuildChildren(List<OperationCategorySum> categories)
    {
        return
        [
            .. categories.Where(x => x.TotalSum > 0)
                .Select(child => new TreeItemData<OperationCategorySum>
                {
                    Text = child.Name,
                    Value = child,
                    Children = child.SubCategories == null ? null : BuildChildren(child.SubCategories),
                })
                .OrderBy(item => item.Value?.TotalSum)
                .ThenBy(item => item.Value?.Name),
        ];
    }

    private List<OperationCategorySum> CalculateCategorySums(List<Category> categories, Dictionary<int, Operation[]> operations, int? parentId)
    {
        var categorySums = new List<OperationCategorySum>();

        foreach (var category in categories.Where(x => x.ParentId == parentId))
        {
            var totalMainSum = category.Id != null && operations.TryGetValue(category.Id.Value, out var operationGroup)
                ? operationGroup.Sum(op => op.Sum)
                : 0;

            var childCategorySums = CalculateCategorySums(categories, operations, category.Id);

            var operationCategorySum = new OperationCategorySum
            {
                Name = category.Name,
                Color = category.Color,
                ParentId = parentId,
                TotalSum = totalMainSum + childCategorySums.Sum(x => x.TotalSum),
                SubCategories = childCategorySums,
            };

            if (childCategorySums.Count > 0)
            {
                operationCategorySum.SubCategories.Add(new()
                {
                    Name = category.Name,
                    TotalSum = totalMainSum,
                });
            }

            categorySums.Add(operationCategorySum);
        }

        return categorySums;
    }
}
