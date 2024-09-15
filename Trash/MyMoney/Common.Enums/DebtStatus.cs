using System.ComponentModel;

namespace Common.Enums
{
    public enum DebtStatus
    {
        [Description("Актуальный")]
        Actual = 1,

        [Description("Уплачен")]
        Paid = 2,

        [Description("Удалён")]
        Deleted = 3,
    }
}
