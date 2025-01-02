using System.ComponentModel;

namespace Money.Business.Enums;

public enum DebtStatus
{
    [Description("Актуальный")]
    Actual = 1,

    [Description("Уплачен")]
    Paid = 2,
}
