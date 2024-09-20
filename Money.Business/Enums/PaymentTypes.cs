using Money.Common;

namespace Money.Business.Enums;

public class PaymentTypes(int value, string name, string description) : Enumeration(value, name)
{
    /// <summary>
    ///     Расходы - 1.
    /// </summary>
    public static readonly PaymentTypes Costs = new(1, nameof(Costs), "Расходы");

    /// <summary>
    ///     Доходы - 2.
    /// </summary>
    public static readonly PaymentTypes Income = new(2, nameof(Income), "Доходы");

    public string Description { get; private set; } = description;

    public static implicit operator PaymentTypes(int value)
    {
        return FromValue<PaymentTypes>(value);
    }

    public static implicit operator PaymentTypes(string name)
    {
        return FromName<PaymentTypes>(name);
    }
}
