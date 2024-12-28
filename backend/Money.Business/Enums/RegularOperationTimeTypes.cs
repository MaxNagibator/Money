using System.ComponentModel;

namespace Money.Business.Enums;

public enum RegularOperationTimeTypes
{
    [Description("Каждый день")]
    EveryDay = 1,

    [Description("Каждую неделю")]
    EveryWeek = 2,

    [Description("Каждый месяц")]
    EveryMonth = 3,
}
