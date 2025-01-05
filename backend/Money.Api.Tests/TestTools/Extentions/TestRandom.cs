namespace Money.Api.Tests.TestTools.Extentions
{
    public static class TestRandom
    {
        private static Random _random = new Random();

        public static int GetInt(int minValue = 0, int maxValue = 606217)
        {
            return _random.Next(minValue, maxValue);
        }

        public static string GetString(string? prefix = null)
        {
            return prefix + Guid.NewGuid();
        }
    }
}
