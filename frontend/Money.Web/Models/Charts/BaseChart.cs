using Money.Web.Models.Charts.Config;
using ChartComponent = Money.Web.Components.Charts.Chart;

namespace Money.Web.Models.Charts;

public abstract class BaseChart
{
    public ChartComponent? Chart { get; set; }
    public required int OperationTypeId { get; set; }
    public required ChartConfig Config { get; set; }
}
