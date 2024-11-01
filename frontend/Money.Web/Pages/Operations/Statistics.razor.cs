using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.BarChart.Axes;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Axes.Ticks;
using ChartJs.Blazor.PieChart;

namespace Money.Web.Pages.Operations;

public partial class Statistics
{
    private static readonly Dictionary<TimeFrame.Mode, TimeFrame> TimeFrames = new()
    {
        [TimeFrame.Mode.Day] = new TimeFrame((operation, date) => operation.Date == date.Date,
            date => date.ToShortDateString(),
            date => date.AddDays(1)),
        [TimeFrame.Mode.Week] = new TimeFrame((operation, date) => operation.Date >= date.Date && operation.Date < date.AddDays(7).Date,
            date => date.ToString("dd MMMM"),
            date => date.AddDays(7)),
        [TimeFrame.Mode.Month] = new TimeFrame((operation, date) => operation.Date >= date.Date && operation.Date < date.AddMonths(1).Date,
            date => date.ToString("MMMM yyyy"),
            date => date.AddMonths(1)),
    };

    private readonly int _daysBreakpoint = 31;
    private readonly int _weekBreakpoint = 140;

    private Dictionary<int, BarChart> _barCharts;
    private Dictionary<int, PieChart> _pieCharts;

    protected override void OnInitialized()
    {
        var barCharts = new Dictionary<int, BarChart>();
        var pieCharts = new Dictionary<int, PieChart>();
        foreach (var operationType in OperationTypes.Values)
        {
            barCharts.Add(operationType.Id, new BarChart
            {
                Chart = new Chart(),
                Config = new BarConfig
                {
                    Options = new BarOptions
                    {
                        Responsive = true,
                        Scales = new BarScales
                        {
                            XAxes = new List<CartesianAxis>
                            {
                                new BarCategoryAxis
                                {
                                    Stacked = true,
                                },
                            },
                            YAxes = new List<CartesianAxis>
                            {
                                new BarLinearCartesianAxis
                                {
                                    Stacked = true,
                                    Ticks = new LinearCartesianTicks
                                    {
                                        BeginAtZero = true,
                                    },
                                },
                            },
                        },
                    },
                },
            });
            pieCharts.Add(operationType.Id, new PieChart
            {
                Chart = new Chart(),
                Config = new PieConfig
                {
                    Options = new PieOptions
                    {
                        Responsive = true,
                        Legend = new ChartJs.Blazor.Common.Legend
                        {
                            Display = true,
                            Position = ChartJs.Blazor.Common.Enums.Position.Right,
                            Labels = new ChartJs.Blazor.Common.LegendLabels
                            {
                                BoxWidth = 50,
                            }
                        }
                    }
                }
            });
        }
        _barCharts = barCharts;
        _pieCharts = pieCharts;
    }

    protected override async void OnSearchChanged(object? sender, List<Operation>? operations)
    {
        foreach (var operationType in OperationTypes.Values)
        {
            var barChart = _barCharts[operationType.Id];
            barChart.Config.Data.Datasets.Clear();
            barChart.Config.Data.Labels.Clear();

            var pieChart = _pieCharts[operationType.Id];
            pieChart.Config.Data.Datasets.Clear();
            pieChart.Config.Data.Labels.Clear();

            if (operations == null)
            {
                return;
            }

            (DateTime start, DateTime finish, double totalDays) = GetOperationsDateRange(operations);
            TimeFrame.Mode mode = DetermineTimeFrame(totalDays, ref start, ref finish);

            List<Category> categories = operations
                .Where(x => x.Category.OperationType.Id == operationType.Id)
                .Select(x => x.Category)
                .DistinctBy(x => x.Id)
                .ToList();

            BarDataset<decimal?>[] datasets = CreateDatasets(barChart, categories);

            TimeFrame timeFrame = TimeFrames[mode];

            do
            {
                barChart.Config.Data.Labels.Add(timeFrame.Labeling.Invoke(start));

                for (int i = 0; i < categories.Count; i++)
                {
                    Category category = categories[i];

                    decimal sum = operations.Where(x => timeFrame.Predicate.Invoke(x, start))
                        .Where(x => x.Category.Id == category.Id)
                        .Sum(x => x.Sum);

                    datasets[i].Add(sum == 0 ? null : sum);
                }

                start = timeFrame.Modifier.Invoke(start);
            } while (start <= finish);

            await barChart.Chart.Update();

            var paymentGroups = operations.GroupBy(x => x.Category.Id!.Value).ToDictionary(x => x.Key, x => x.ToArray());
            var cats = GetValue(categories, operationType.Id, paymentGroups, null);

            var ds = new PieDataset<decimal>();
            pieChart.Config.Data.Datasets.Add(ds);

            var notZeroCategories = cats.Where(x => x.ParentId == null && x.TotalSum != 0);
            var colors = new List<string>();
            foreach (var category in notZeroCategories)
            {
                pieChart.Config.Data.Labels.Add(category.Name);
                colors.Add(category.Color ?? Random.Shared.NextColor());
                ds.Add(category.TotalSum);
            }
            ds.BackgroundColor = colors.ToArray();

            await pieChart.Chart.Update();
        }
    }

    private BarDataset<decimal?>[] CreateDatasets(BarChart barChart, List<Category> categories)
    {
        BarDataset<decimal?>[] datasets = new BarDataset<decimal?>[categories.Count];

        for (int i = 0; i < categories.Count; i++)
        {
            Category category = categories[i];

            BarDataset<decimal?> dataset = new()
            {
                Label = category.Name,
                BackgroundColor = category.Color ?? Random.Shared.NextColor(),
            };

            datasets[i] = dataset;
            barChart.Config.Data.Datasets.Add(dataset);
        }

        return datasets;
    }

    private TimeFrame.Mode DetermineTimeFrame(double totalDays, ref DateTime start, ref DateTime finish)
    {
        if (totalDays <= _daysBreakpoint)
        {
            return TimeFrame.Mode.Day;
        }

        if (totalDays <= _weekBreakpoint)
        {
            start = start.StartOfWeek();
            finish = finish.StartOfWeek();
            return TimeFrame.Mode.Week;
        }

        start = start.StartOfMonth();
        finish = finish.StartOfMonth();
        return TimeFrame.Mode.Month;
    }

    private (DateTime start, DateTime finish, double totalDays) GetOperationsDateRange(List<Operation> operations)
    {
        DateTime start = OperationsFilter.DateRange.Start ?? DateTime.Now;
        DateTime finish = OperationsFilter.DateRange.End ?? DateTime.Now;

        if ((finish - start).TotalDays > 10)
        {
            start = operations.MinBy(x => x.Date)?.Date ?? DateTime.Now;
            finish = operations.MaxBy(x => x.Date)?.Date ?? DateTime.Now;
        }

        return (start, finish, (finish - start).TotalDays);
    }

    private List<OperationCategorySum> GetValue(List<Category> categories,
        int operationTypeId,
        Dictionary<int, Operation[]> paymentGroups,
        int? parentId)
    {
        List<OperationCategorySum> cats = new List<OperationCategorySum>();
        foreach (var category in categories.Where(x => x.OperationType.Id == operationTypeId && x.ParentId == parentId))
        {
            var cx = new OperationCategorySum();
            cx.Name = category.Name;
            cx.Color = category.Color;
            cx.ParentId = parentId;
            decimal mainSum;
            if (paymentGroups.ContainsKey(category.Id!.Value))
            {
                mainSum = paymentGroups[category.Id!.Value].Sum(x => x.Sum);
            }
            else
            {
                mainSum = 0;
            }
            cx.TotalSum = mainSum;
            var chields = GetValue(categories, operationTypeId, paymentGroups, category.Id);
            cx.TotalSum += chields.Sum(x => x.TotalSum);
            cx.Chields = chields;
            if (chields.Count > 0)
            {
                cx.Chields.Add(new OperationCategorySum { TotalSum = mainSum, Name = category.Name });
            }
            cats.Add(cx);
        }
        return cats;
    }

    private record TimeFrame(
        Func<Operation, DateTime, bool> Predicate,
        Func<DateTime, string> Labeling,
        Func<DateTime, DateTime> Modifier)
    {
        public enum Mode
        {
            Day = 0,
            Week = 1,
            Month = 2,
        }
    }

    public class BarChart
    {
        public Chart Chart { get; set; }
        public BarConfig Config { get; set; }
    }

    public class PieChart
    {
        public Chart Chart { get; set; }
        public PieConfig Config { get; set; }
    }

    public class OperationCategorySum
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public decimal MainSum { get; set; }
        public decimal TotalSum { get; set; }
        public int? ParentId { get; set; }
        public List<OperationCategorySum> Chields { get; set; }
    }
}
