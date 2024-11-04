using ChartJs.Blazor.Common;
using ChartJs.Blazor.PieChart;
using Money.Web.Models.Charts;

namespace Money.Web.Pages.Operations;

public partial class Statistics
{
    private Dictionary<int, BarChart> _barCharts = default!;
    private Dictionary<int, PieChart> _pieCharts = default!;

    protected override void OnInitialized()
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
    }

    protected override async void OnSearchChanged(object? sender, List<Operation>? operations)
    {
        List<Task> tasks = [];

        Dictionary<int, Operation[]>? operationGroups = operations?
            .GroupBy(x => x.Category.Id!.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());

        foreach (OperationTypes.Value operationType in OperationTypes.Values)
        {
            List<Category> categories = operations?
                                            .Where(x => x.Category.OperationType.Id == operationType.Id)
                                            .Select(x => x.Category)
                                            .DistinctBy(x => x.Id)
                                            .ToList()
                                        ?? [];

            tasks.Add(_barCharts[operationType.Id].Update(operations, categories, OperationsFilter.DateRange));
            tasks.Add(GeneratePieChart(operationGroups, categories, operationType));
        }

        await Task.WhenAll(tasks);
    }

    private Task GeneratePieChart(Dictionary<int, Operation[]>? operations, List<Category> categories, OperationTypes.Value operationType)
    {
        PieChart chart = _pieCharts[operationType.Id];
        ChartData configData = chart.Config.Data;

        configData.Datasets.Clear();
        configData.Labels.Clear();

        if (operations != null)
        {
            FillPieChart(configData, operations, categories, operationType);
        }

        return chart.Chart.Update();
    }

    private void FillPieChart(ChartData configData, Dictionary<int, Operation[]> operations, List<Category> categories, OperationTypes.Value operationType)
    {
        List<CategoryOperationSum> categorySums = CalculateCategorySums(categories, operationType.Id, operations, null);

        PieDataset<decimal> dataset = [];
        configData.Datasets.Add(dataset);

        List<string> colors = [];

        foreach (CategoryOperationSum category in categorySums.Where(x => x.ParentId == null && x.TotalSum != 0))
        {
            configData.Labels.Add(category.Name);
            colors.Add(category.Color ?? Random.Shared.NextColor());
            dataset.Add(category.TotalSum);
        }

        dataset.BackgroundColor = colors.ToArray();
    }

    private List<CategoryOperationSum> CalculateCategorySums(List<Category> categories, int operationTypeId, Dictionary<int, Operation[]> operations, int? parentId)
    {
        List<CategoryOperationSum> categorySums = [];

        foreach (Category category in categories.Where(x => x.OperationType.Id == operationTypeId && x.ParentId == parentId))
        {
            decimal totalMainSum = category.Id != null && operations.TryGetValue(category.Id.Value, out Operation[]? operationGroup)
                ? operationGroup.Sum(op => op.Sum)
                : 0;

            List<CategoryOperationSum> childCategorySums = CalculateCategorySums(categories, operationTypeId, operations, category.Id);

            CategoryOperationSum categorySum = new()
            {
                Name = category.Name,
                Color = category.Color,
                ParentId = parentId,
                TotalSum = totalMainSum + childCategorySums.Sum(x => x.TotalSum),
                SubCategories = childCategorySums,
            };

            if (childCategorySums.Count > 0)
            {
                categorySum.SubCategories.Add(new CategoryOperationSum
                {
                    Name = category.Name,
                    TotalSum = totalMainSum,
                });
            }

            categorySums.Add(categorySum);
        }

        return categorySums;
    }

    public class CategoryOperationSum
    {
        public required string Name { get; set; }
        public string? Color { get; set; }
        public decimal MainSum { get; set; }
        public decimal TotalSum { get; set; }
        public int? ParentId { get; set; }
        public List<CategoryOperationSum>? SubCategories { get; set; }
    }
}
