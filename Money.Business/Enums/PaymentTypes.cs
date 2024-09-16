using System.ComponentModel;

namespace Money.Business.Enums;

public enum PaymentTypes
{
    [Description("Расходы")]
    Costs = 1,

    [Description("Доходы")]
    Income = 2,
}
