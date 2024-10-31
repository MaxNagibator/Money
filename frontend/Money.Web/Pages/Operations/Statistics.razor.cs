using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.BarChart.Axes;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Axes.Ticks;
using Microsoft.AspNetCore.Components;
using Money.Web.Components;

namespace Money.Web.Pages.Operations;

public partial class Statistics
{
    private BarConfig _config;
    private Chart _chart;

    [CascadingParameter]
    public OperationsFilter OperationsFilter { get; set; } = default!;

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

        // Просто набросок для проверки работоспособности
        OperationsFilter.OnSearch += async (sender, list) =>
        {
            _config.Data.Datasets.Clear();
            _config.Data.Labels.Clear();
            var mode = "day";

            var date1 = OperationsFilter.DateRange.Start ?? DateTime.Now;
            var date2 = OperationsFilter.DateRange.End ?? DateTime.Now;
            if ((date2 - date1).TotalDays > 10)
            {
                date1 = list!.Select(x => x.Date).DefaultIfEmpty(DateTime.Now).Min();
                date2 = list!.Select(x => x.Date).DefaultIfEmpty(DateTime.Now).Max();
            }
            var totalDays = (date2 - date1).TotalDays;
            if (totalDays > 31)
            {
                if (totalDays > 140)
                {
                    date1 = new DateTime(date1.Year, date1.Month, 1);
                    date2 = new DateTime(date2.Year, date2.Month, 1);
                    mode = "month";
                }
                else
                {
                    int diff = (7 + (date1.DayOfWeek - DayOfWeek.Monday)) % 7;
                    date1 = date1.AddDays(-1 * diff).Date;
                    int diff2 = (7 + (date2.DayOfWeek - DayOfWeek.Monday)) % 7;
                    date2 = date2.AddDays(-1 * diff2).Date;

                    mode = "week";
                }
            }

            var categories = list!.Where(x => x.Category.OperationType.Id == 1).Select(x => x.Category).DistinctBy(x => x.Id).ToList();
            //
            BarDataset<decimal?>[] datasets = new BarDataset<decimal?>[categories.Count];
            for (int i = 0; i < categories.Count; i++)
            {
                Category category = categories[i];
                BarDataset<decimal?> dataset = new BarDataset<decimal?>()
                {
                    Label = category.Name,
                    BackgroundColor = category.Color ?? Random.Shared.NextColor(),

                };
                datasets[i] = dataset;
                _config.Data.Datasets.Add(dataset);
            }
            while (true)
            {
                // todo криво но пойдёт, подрефачить

                string label22;
                IEnumerable<Operation> sum22;
                if (mode == "day")
                {
                    sum22 = list!.Where(x => x.Date == date1.Date);
                    label22 = date1.ToShortDateString();
                }
                else if (mode == "week")
                {
                    sum22 = list!.Where(x => x.Date >= date1.Date && x.Date < date1.AddDays(7).Date);
                    label22 = date1.ToString("dd MMMM");
                }
                else
                {
                    sum22 = list!.Where(x => x.Date >= date1.Date && x.Date < date1.AddMonths(1).Date);
                    label22 = date1.ToString("MMMM yyyy");
                }
                _config.Data.Labels.Add(label22);

                for (int i = 0; i < categories.Count; i++)
                {
                    Category? operGroup = categories[i];
                    var operationsByGroup = sum22.Where(x => x.Category.Id == operGroup.Id).Sum(x => x.Sum);
                    datasets[i].Add(operationsByGroup == 0 ? null : operationsByGroup);
                }
                if (mode == "day")
                {
                    date1 = date1.AddDays(1);
                }
                else if (mode == "week")
                {
                    date1 = date1.AddDays(7);
                }
                else
                {
                    date1 = date1.AddMonths(1);
                }
                if (date1 > date2)
                {
                    break;
                }
            }

            await _chart.Update();
        };
    }
}
