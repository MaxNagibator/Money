using Money.Web.Models.Charts.Config;

namespace Money.Web.Models.Charts;

public sealed class PieChart : BaseChart
{
    private ChartData Data => Config.Data;

    public static PieChart Create(int operationTypeId, bool useThemeColors = true)
    {
        var legend = new ChartLegend
        {
            Display = true,
            Position = "right",
            Labels = new()
            {
                BoxWidth = 50,
            },
        };

        if (useThemeColors)
        {
            legend.Labels.Color = "var(--mud-palette-text-primary)";
        }

        return new()
        {
            Chart = null,
            Config = new()
            {
                Type = "pie",
                Options = new()
                {
                    Responsive = true,
                    Plugins = new()
                    {
                        Legend = legend,
                    },
                },
            },
            OperationTypeId = operationTypeId,
        };
    }

    // TODO: Не работает + дублирование
    public void UpdateTheme(bool useThemeColors)
    {
        var legend = Config.Options.Plugins?.Legend;

        if (legend?.Labels == null)
        {
            return;
        }

        if (useThemeColors)
        {
            legend.Labels.Color = "var(--mud-palette-text-primary)";
        }
        else
        {
            legend.Labels.Color = null;
        }
    }

    public ValueTask UpdateAsync(List<OperationCategorySum> operations)
    {
        Data.Datasets.Clear();
        Data.Labels.Clear();

        FillPieChart(operations);

        if (Chart != null)
        {
            return Chart.UpdateAsync();
        }

        return ValueTask.CompletedTask;
    }

    private void FillPieChart(List<OperationCategorySum> categorySums)
    {
        var dataset = new ChartDataset();
        Data.Datasets.Add(dataset);

        List<string> colors = [];
        var index = 0;

        foreach (var category in categorySums.Where(x => x.ParentId == null && x.TotalSum != 0))
        {
            Data.Labels.Add(category.Name);
            colors.Add(category.Color ?? ChartColors.GetColor(index++));
            dataset.Add(category.TotalSum);
        }

        dataset.BackgroundColor = colors.ToArray();
    }
}
