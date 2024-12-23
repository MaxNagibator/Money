using Money.Common;
using System.ComponentModel;

namespace Money.Business.Enums;

public enum RegularTaskTimeTypes
{
    [Description("Каждый день")]
    EveryDay = 1,

    [Description("Каждую неделю")]
    EveryWeek = 2,

    [Description("Каждый месяц")]
    EveryMonth = 3,
}
