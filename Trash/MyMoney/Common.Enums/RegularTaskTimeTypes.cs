using System.ComponentModel;

namespace Common.Enums
{
    public enum RegularTaskTimeTypes
    {
        [Description("Каждый день")]
        EveryDay = 1,

        [Description("Каждую неделю")]
        EveryWeek = 2,

        [Description("Каждый месяц")]
        EveryMonth = 3,
    }
}
