namespace Money.Web.Models.Charts;

public record OperationTypeStatistics(BarChart BarChart, PieChart PieChart)
{
    public MudTreeView<OperationCategorySum>? TreeView { get; set; }
    public List<TreeItemData<OperationCategorySum>> Sums { get; set; } = [];
}
