using ChartJs.Blazor.Common;
using ChartJs.Blazor.PieChart;
using Position = ChartJs.Blazor.Common.Enums.Position;

namespace Money.Web.Models.Charts;

public class PieChart : BaseChart<PieOptions>
{
    private ChartData Data => Config.Data;

    public static PieChart Create(int operationTypeId)
    {
        return new()
        {
            Chart = new(),
            Config = new PieConfig
            {
                Options = new()
                {
                    Responsive = true,
                    Legend = new()
                    {
                        Display = true,
                        Position = Position.Right,
                        Labels = new()
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
        Data.Datasets.Clear();
        Data.Labels.Clear();

        FillPieChart(operations);

        return Chart.Update();
    }

    private void FillPieChart(List<OperationCategorySum> categorySums)
    {
        PieDataset<decimal> dataset = [];
        Data.Datasets.Add(dataset);

        List<string> colors = [];

        foreach (var category in categorySums.Where(x => x.ParentId == null && x.TotalSum != 0))
        {
            Data.Labels.Add(category.Name);
            colors.Add(category.Color ?? Random.Shared.NextColor());
            dataset.Add(category.TotalSum);
        }

        dataset.BackgroundColor = colors.ToArray();
    }
}
