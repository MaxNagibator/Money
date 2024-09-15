using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Common.Enums
{
    public enum PaymentTypes
    {
        [Description("Расходы")]
        Costs = 1,

        [Description("Доходы")]
        Income = 2,
    }
}
