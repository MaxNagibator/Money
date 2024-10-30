using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.BarChart.Axes;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Enums;
using Microsoft.AspNetCore.Components;
using Money.Web.Components;

namespace Money.Web.Pages.Operations;

public partial class Statistics
{
    private BarConfig _config;
    private Chart _chart;

    [CascadingParameter]
    public PaymentsFilter PaymentsFilter { get; set; } = default!;

    protected override void OnInitialized()
    {
        _config = new BarConfig
        {
            Options = new BarOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Chart.js Bar Chart - Stacked",
                },
                Tooltips = new Tooltips
                {
                    Mode = InteractionMode.Index,
                    Intersect = false,
                },
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
                        },
                    },
                },
            },
        };

        // Просто набросок для проверки работоспособности
        PaymentsFilter.OnSearch += async (sender, list) =>
        {
            _config.Data.Datasets.Clear();

            if (list != null && list.Count != 0)
            {
                foreach (IGrouping<Category, Payment> group in list.Where(x => x.Category.PaymentType.Id == 1).GroupBy(x => x.Category))
                {
                    IDataset<int> dataset = new BarDataset<int>(group.Select(x => (int)x.Sum).ToList())
                    {
                        Label = group.Key.Name,
                        BackgroundColor = group.Key.Color,
                    };

                    _config.Data.Datasets.Add(dataset);
                }
            }

            await _chart.Update();
        };
    }
}
