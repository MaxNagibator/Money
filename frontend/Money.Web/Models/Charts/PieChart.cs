using ChartJs.Blazor;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.PieChart;
using Position = ChartJs.Blazor.Common.Enums.Position;

namespace Money.Web.Models.Charts;

public class PieChart : BaseChart<PieOptions>
{
    public static PieChart Create(int operationTypeId)
    {
        return new PieChart
        {
            Chart = new Chart(),
            Config = new PieConfig
            {
                Options = new PieOptions
                {
                    Responsive = true,
                    Legend = new Legend
                    {
                        Display = true,
                        Position = Position.Right,
                        Labels = new LegendLabels
                        {
                            BoxWidth = 50,
                        },
                    },
                },
            },
            OperationTypeId = operationTypeId,
        };
    }

    public Task UpdateAsync(List<OperationCategorySum> operations)
    {
        ChartData configData = Config.Data;
        configData.Datasets.Clear();
        configData.Labels.Clear();

        if (operations != null)
        {
            FillPieChart(configData, operations);
        }

        return Chart.Update();
    }

    private void FillPieChart(ChartData configData, List<OperationCategorySum> categorySums)
    {
        PieDataset<decimal> dataset = [];
        configData.Datasets.Add(dataset);

        List<string> colors = [];

        foreach (OperationCategorySum category in categorySums.Where(x => x.ParentId == null && x.TotalSum != 0))
        {
            configData.Labels.Add(category.Name);
            colors.Add(category.Color ?? Random.Shared.NextColor());
            dataset.Add(category.TotalSum);
        }

        dataset.BackgroundColor = colors.ToArray();
    }
}
