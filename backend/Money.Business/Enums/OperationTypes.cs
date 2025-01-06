using System.ComponentModel;

namespace Money.Business.Enums;

public enum OperationTypes
{
    /// <summary>
    /// Расходы.
    /// </summary>
    [Description("Расходы")]
    Costs = 1,

    /// <summary>
    /// Доходы.
    /// </summary>
    [Description("Доходы")]
    Income = 2,
}
