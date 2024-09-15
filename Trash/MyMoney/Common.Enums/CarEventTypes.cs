using System.ComponentModel;

namespace Common.Enums
{
    public enum CarEventTypes
    {
        [Description("Приобретение")]
        Buy = 1,

        [Description("Страховка")]
        Strahovka = 2,

        [Description("Обсулживание")]
        Service = 3,

        [Description("Прочее")]
        Other = 99,
    }
}
