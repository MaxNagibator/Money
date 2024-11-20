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

    public Task UpdateAsync(Dictionary<int, Operation[]>? operations, List<Category> categories)
    {
        ChartData configData = Config.Data;
        configData.Datasets.Clear();
        configData.Labels.Clear();

        if (operations != null)
        {
            FillPieChart(configData, operations, categories);
        }

        return Chart.Update();
    }

    private void FillPieChart(ChartData configData, Dictionary<int, Operation[]> operations, List<Category> categories)
    {
        List<OperationCategorySum> categorySums = CalculateCategorySums(categories, operations, null);

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

    private List<OperationCategorySum> CalculateCategorySums(List<Category> categories, Dictionary<int, Operation[]> operations, int? parentId)
    {
        List<OperationCategorySum> categorySums = [];

        foreach (Category category in categories.Where(x => x.ParentId == parentId))
        {
            decimal totalMainSum = category.Id != null && operations.TryGetValue(category.Id.Value, out Operation[]? operationGroup)
                ? operationGroup.Sum(op => op.Sum)
                : 0;

            List<OperationCategorySum> childCategorySums = CalculateCategorySums(categories, operations, category.Id);

            OperationCategorySum operationCategorySum = new()
            {
                Name = category.Name,
                Color = category.Color,
                ParentId = parentId,
                TotalSum = totalMainSum + childCategorySums.Sum(x => x.TotalSum),
                SubCategories = childCategorySums,
            };

            if (childCategorySums.Count > 0)
            {
                operationCategorySum.SubCategories.Add(new OperationCategorySum
                {
                    Name = category.Name,
                    TotalSum = totalMainSum,
                });
            }

            categorySums.Add(operationCategorySum);
        }

        return categorySums;
    }

    private class OperationCategorySum
    {
        public required string Name { get; init; }
        public string? Color { get; init; }
        public decimal MainSum { get; init; }
        public decimal TotalSum { get; init; }
        public int? ParentId { get; init; }
        public List<OperationCategorySum>? SubCategories { get; init; }
    }
}
