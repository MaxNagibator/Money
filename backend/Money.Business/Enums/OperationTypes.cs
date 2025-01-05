using System.ComponentModel;

namespace Money.Business.Enums;

public enum OperationTypes
{
    [Description("Расходы")]
    Costs = 1,

    [Description("Доходы")]
    Income = 2,
}
