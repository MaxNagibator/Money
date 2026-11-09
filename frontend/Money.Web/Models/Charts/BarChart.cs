using Money.Web.Models.Charts.Config;
using ChartOptions = Money.Web.Models.Charts.Config.ChartOptions;

namespace Money.Web.Models.Charts;

public class BarChart : BaseChart
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

    public static BarChart Create(int operationTypeId, bool useThemeColors = true)
    {
        return new()
        {
            Chart = null,
            Config = new()
            {
                Type = "bar",
                Options = BuildOptions(useThemeColors),
            },
            OperationTypeId = operationTypeId,
        };
    }

    public void ApplyTheme(bool useThemeColors)
    {
        Config.Options = BuildOptions(useThemeColors);
    }

    public ValueTask UpdateAsync(List<Operation>? operations, List<Category> categories, DateRange range)
    {
        Config.Data.Datasets.Clear();
        Config.Data.Labels.Clear();

        if (operations != null)
        {
            FillBarChart(Config.Data, operations, categories, range);
        }

        if (Chart != null)
        {
            return Chart.UpdateAsync();
        }

        return ValueTask.CompletedTask;
    }

    private static ChartOptions BuildOptions(bool useThemeColors)
    {
        var scales = new Dictionary<string, ChartAxis>
        {
            ["x"] = new()
            {
                Stacked = true,
            },
            ["y"] = new()
            {
                Stacked = true,
                BeginAtZero = true,
            },
        };

        var legend = new ChartLegend
        {
            Display = true,
            Position = "top",
            Labels = new(),
        };

        if (useThemeColors)
        {
            scales["x"].Ticks = new()
            {
                Color = "var(--mud-palette-text-secondary)",
            };

            scales["x"].Grid = new()
            {
                Color = "var(--mud-palette-lines-default)",
            };

            scales["y"].Ticks = new()
            {
                Color = "var(--mud-palette-text-secondary)",
            };

            scales["y"].Grid = new()
            {
                Color = "var(--mud-palette-lines-default)",
            };

            legend.Labels.Color = "var(--mud-palette-text-primary)";
        }

        return new()
        {
            Responsive = true,
            Scales = scales,
            Plugins = new()
            {
                Legend = legend,
            },
        };
    }

    private static ChartDataset[] CreateDatasets(ChartData configData, List<Category> categories)
    {
        var datasets = new ChartDataset[categories.Count];

        for (var i = 0; i < categories.Count; i++)
        {
            var category = categories[i];

            var dataset = new ChartDataset
            {
                Label = category.Name,
                BackgroundColor = category.Color ?? ChartColors.GetColor(i),
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
        var (start, finish, totalDays) = GetOperationsDateRange(operations, range);
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

    private sealed record TimeFrame(
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
