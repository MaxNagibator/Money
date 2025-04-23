namespace Money.Web.Common;

public static class MoneyExtensions
{
    public static string ToMoneyString(this decimal value)
    {
        return value.ToString("N2");
    }
}
