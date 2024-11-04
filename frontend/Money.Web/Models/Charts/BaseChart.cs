using ChartJs.Blazor;
using ChartJs.Blazor.Common;

namespace Money.Web.Models.Charts;

public abstract class BaseChart<T> where T : BaseConfigOptions
{
    public required Chart Chart { get; set; }
    public required int OperationTypeId { get; set; }
    public required ConfigBase<T> Config { get; set; }
}
