using Money.Common;

namespace Money.Business.Enums;

public class PaymentTypes : Enumeration
{
    public static readonly PaymentTypes Costs = new(1, nameof(Costs), "Расходы");
    public static readonly PaymentTypes Income = new(2, nameof(Income), "Доходы");

    private PaymentTypes(int value, string name, string description) : base(value, name)
    {
        Description = description;
    }

    public string Description { get; private set; }

    public static implicit operator PaymentTypes(int value)
    {
        return FromValue<PaymentTypes>(value);
    }

    public static implicit operator PaymentTypes(string name)
    {
        return FromName<PaymentTypes>(name);
    }
}
