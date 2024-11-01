using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.BarChart.Axes;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Axes.Ticks;

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

    private BarConfig _config;
    private Chart _chart;

    protected override void OnInitialized()
    {
        _config = new BarConfig
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
        };
    }

    protected override async void OnSearchChanged(object? sender, List<Operation>? operations)
    {
        _config.Data.Datasets.Clear();
        _config.Data.Labels.Clear();

        if (operations == null)
        {
            return;
        }

        (DateTime start, DateTime finish, double totalDays) = GetOperationsDateRange(operations);
        TimeFrame.Mode mode = DetermineTimeFrame(totalDays, ref start, ref finish);

        List<Category> categories = operations
            .Where(x => x.Category.OperationType.Id == 1)
            .Select(x => x.Category)
            .DistinctBy(x => x.Id)
            .ToList();

        BarDataset<decimal?>[] datasets = CreateDatasets(categories);

        TimeFrame timeFrame = TimeFrames[mode];

        do
        {
            _config.Data.Labels.Add(timeFrame.Labeling.Invoke(start));

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

        await _chart.Update();
    }

    private BarDataset<decimal?>[] CreateDatasets(List<Category> categories)
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
            _config.Data.Datasets.Add(dataset);
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
}
