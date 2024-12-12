namespace Money.Web.Models;

public class OperationsDay
{
    public DateTime Date { get; set; }

    public List<Operation> Operations { get; set; } = [];

    public Action<Operation> AddAction { get; set; } = _ => { };

    public decimal CalculateSum(OperationTypes.Value operationType)
    {
        return Operations.Where(x => x.IsDeleted == false && x.Category.OperationType == operationType)
            .Sum(x => x.Sum);
    }
}
