using Microsoft.AspNetCore.Components;
using Money.Web.Models;
using Money.Web.Models.Charts;

namespace Money.Web.Pages.Operations;

public partial class Statistics
{
    private Dictionary<int, BarChart> _barCharts = default!;
    private Dictionary<int, PieChart> _pieCharts = default!;
    public List<Category>? Categories { get; private set; }

    private Dictionary<int, List<TreeItemData<OperationCategorySum>>> _sums { get; } = [];

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Dictionary<int, BarChart> barCharts = new();
        Dictionary<int, PieChart> pieCharts = new();

        foreach (OperationTypes.Value operationType in OperationTypes.Values)
        {
            barCharts.Add(operationType.Id, BarChart.Create(operationType.Id));
            pieCharts.Add(operationType.Id, PieChart.Create(operationType.Id));
        }

        _barCharts = barCharts;
        _pieCharts = pieCharts;
        Categories = await CategoryService.GetCategories();
    }

    protected override void OnSearchChanged(object? sender, OperationSearchEventArgs args)
    {
        List<Task> tasks = [];

        Dictionary<int, Operation[]>? operationGroups = args.Operations?
            .GroupBy(x => x.Category.Id!.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());

        foreach (OperationTypes.Value operationType in OperationTypes.Values)
        {
            List<Category> categories = args.Operations?
                                            .Where(x => x.Category.OperationType.Id == operationType.Id)
                                            .Select(x => x.Category)
                                            .DistinctBy(x => x.Id)
                                            .ToList()
                                        ?? [];

            tasks.Add(_barCharts[operationType.Id].UpdateAsync(args.Operations, categories, OperationsFilter.DateRange));

            if (Categories != null)
            {
                // todo над жать найти, чтоб отрисовалось :)
                var cats = Categories.Where(x => x.OperationType.Id == operationType.Id).ToList();
                Console.WriteLine(cats.Count);
                List<OperationCategorySum> categorySums = CalculateCategorySums(cats, operationGroups, null);
                Console.WriteLine(categorySums.Count);

                tasks.Add(_pieCharts[operationType.Id].UpdateAsync(categorySums));
                var sums = BuildChildren(categorySums);
                _sums[operationType.Id] = new List<TreeItemData<OperationCategorySum>>
                {
                    new TreeItemData<OperationCategorySum>
                    {
                        Value = new OperationCategorySum { TotalSum = sums.Sum(x => x.Value.TotalSum), Name = "Всего" },
                        Children = sums,
                    }
                };
            }
        }

        _ = Task.WhenAll(tasks);
    }

    private List<TreeItemData<OperationCategorySum>> BuildChildren(List<OperationCategorySum> categories)
    {
        return categories.Where(x => x.TotalSum > 0)
            .Select(child => new TreeItemData<OperationCategorySum>
            {
                Text = child.Name,
                Value = child,
                Children = child.SubCategories == null ? null : BuildChildren(child.SubCategories),
            })
            .OrderBy(item => item.Value?.TotalSum)
            .ThenBy(item => item.Value?.Name)
            .ToList();
    }

    private List<OperationCategorySum> CalculateCategorySums(List<Category> categories, Dictionary<int, Operation[]> operations, int? parentId)
    {
        List<OperationCategorySum> categorySums = [];

        foreach (Category category in categories.Where(x => x.ParentId == parentId))
        {
            decimal totalMainSum = category.Id != null && operations.TryGetValue(category.Id.Value, out Operation[]? operationGroup)
                ? operationGroup.Sum(op => op.Sum)
                : 0;

            List<OperationCategorySum> childCategorySums = CalculateCategorySums(categories, operations, category.Id);

            OperationCategorySum operationCategorySum = new()
            {
                Name = category.Name,
                Color = category.Color,
                ParentId = parentId,
                TotalSum = totalMainSum + childCategorySums.Sum(x => x.TotalSum),
                SubCategories = childCategorySums,
            };

            if (childCategorySums.Count > 0)
            {
                operationCategorySum.SubCategories.Add(new OperationCategorySum
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
