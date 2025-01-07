using System.ComponentModel;

namespace Money.Business.Enums;

public enum DebtStatus
{
    /// <summary>
    /// Актуальный.
    /// </summary>
    [Description("Актуальный")]
    Actual = 1,

    /// <summary>
    /// Уплачен.
    /// </summary>
    [Description("Уплачен")]
    Paid = 2,
}
