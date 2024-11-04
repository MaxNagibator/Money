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
}
