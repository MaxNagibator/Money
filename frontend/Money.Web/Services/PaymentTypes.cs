
namespace Money.Web.Services
{
    public static class PaymentTypes
    {
        public static Value[] Values { get; } = GetValues();

        // представим, что мы их получили с бэкэнда
        private static Value[] GetValues()
        {
            var values = new List<Value>();
            values.Add(new Value { Id = 1, Name = "Расходы" });
            values.Add(new Value { Id = 2, Name = "Доходы" });
            return values.ToArray();
        }

        public class Value
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
