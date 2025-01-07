using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.BarChart.Axes;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;

namespace Money.Web.Models.Charts;

public class BarChart : BaseChart<BarOptions>
{
    private static readonly Dictionary<TimeFrame.Mode, TimeFrame> TimeFrames = new()
    {
        [TimeFrame.Mode.Day] = new((operation, date) => operation.Date == date.Date,
            date => date.ToShortDateString(),
            date => date.AddDays(1)),
        [TimeFrame.Mode.Week] = new((operation, date) => operation.Date >= date.Date && operation.Date < date.AddDays(7).Date,
            date => date.ToString("dd MMMM"),
            date => date.AddDays(7)),
        [TimeFrame.Mode.Month] = new((operation, date) => operation.Date >= date.Date && operation.Date < date.AddMonths(1).Date,
            date => date.ToString("MMMM yyyy"),
            date => date.AddMonths(1)),
    };

    private readonly int _daysBreakpoint = 31;
    private readonly int _weekBreakpoint = 140;

    public static BarChart Create(int operationTypeId)
    {
        return new()
        {
            Chart = new(),
            Config = new BarConfig
            {
                Options = new()
                {
                    Responsive = true,
                    Scales = new()
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
                                Ticks = new()
                                {
                                    BeginAtZero = true,
                                },
                            },
                        },
                    },
                },
            },
            OperationTypeId = operationTypeId,
        };
    }

    public Task UpdateAsync(List<Operation>? operations, List<Category> categories, DateRange range)
    {
        Config.Data.Datasets.Clear();
        Config.Data.Labels.Clear();

        if (operations != null)
        {
            FillBarChart(Config.Data, operations, categories, range);
        }

        return Chart.Update();
    }

    private static BarDataset<decimal?>[] CreateDatasets(ChartData configData, List<Category> categories)
    {
        var datasets = new BarDataset<decimal?>[categories.Count];

        for (var i = 0; i < categories.Count; i++)
        {
            var category = categories[i];

            var dataset = new BarDataset<decimal?>
            {
                Label = category.Name,
                BackgroundColor = category.Color ?? Random.Shared.NextColor(),
            };

            datasets[i] = dataset;
            configData.Datasets.Add(dataset);
        }

        return datasets;
    }

    private static (DateTime start, DateTime finish, double totalDays) GetOperationsDateRange(List<Operation> operations, DateRange range)
    {
        var start = range.Start ?? DateTime.Now;
        var finish = range.End ?? DateTime.Now;

        if ((finish - start).TotalDays > 10)
        {
            start = operations.MinBy(x => x.Date)?.Date ?? DateTime.Now;
            finish = operations.MaxBy(x => x.Date)?.Date ?? DateTime.Now;
        }

        return (start, finish, (finish - start).TotalDays);
    }

    private void FillBarChart(ChartData configData, List<Operation> operations, List<Category> categories, DateRange range)
    {
        (var start, var finish, var totalDays) = GetOperationsDateRange(operations, range);
        var mode = DetermineTimeFrame(totalDays, ref start, ref finish);

        var datasets = CreateDatasets(configData, categories);

        var timeFrame = TimeFrames[mode];

        do
        {
            configData.Labels.Add(timeFrame.Labeling.Invoke(start));

            for (var i = 0; i < categories.Count; i++)
            {
                var category = categories[i];

                var sum = operations
                    .Where(x => timeFrame.Predicate.Invoke(x, start))
                    .Where(x => x.Category.Id == category.Id)
                    .Sum(x => x.Sum);

                datasets[i].Add(sum == 0 ? null : sum);
            }

            start = timeFrame.Modifier.Invoke(start);
        } while (start <= finish);
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
