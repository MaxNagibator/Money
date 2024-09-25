namespace Money.Web.Models;

public static class PaymentTypes
{
    public static Value[] Values { get; } = GetValues();

    // представим, что мы их получили с бэкэнда
    private static Value[] GetValues()
    {
        return
        [
            new Value { Id = 1, Name = "Расходы" },
            new Value { Id = 2, Name = "Доходы" }
        ];
    }

    public class Value
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
