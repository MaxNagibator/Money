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
            tasks.Add(_pieCharts[operationType.Id].UpdateAsync(operationGroups, categories));
        }

        _ = Task.WhenAll(tasks);
    }
}
