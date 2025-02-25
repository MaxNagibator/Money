using System.ComponentModel;

namespace Money.Business.Enums;

public enum CarEventTypes
{
    /// <summary>
    /// Приобретение.
    /// </summary>
    Buy = 1,

    /// <summary>
    /// Страховка.
    /// </summary>
    Strahovka = 2, // не переименовывать, дань прошлому

    /// <summary>
    /// Обслуживание.
    /// </summary>
    Service = 3,

    /// <summary>
    /// Переобувка.
    /// </summary>
    TireMontate = 4,

    /// <summary>
    /// Прочее.
    /// </summary>
    Other = 99,
}
