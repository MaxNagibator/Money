using Money.Business.Enums;

namespace Money.Business.Models;

/// <summary>
/// Групповой возврат долгов держателя.
/// </summary>
public class DebtOwnerPayment : DebtPayment
{
    /// <summary>
    /// Ожидаемое имя держателя долга.
    /// </summary>
    public required string OwnerName { get; set; }

    /// <summary>
    /// Ожидаемый тип долга.
    /// </summary>
    public DebtTypes Type { get; set; }
}
