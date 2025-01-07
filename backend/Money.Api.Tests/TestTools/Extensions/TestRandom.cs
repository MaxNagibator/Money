namespace Money.Api.Tests.TestTools.Extensions;

public static class TestRandom
{
    private static readonly Random Random = new();

    public static int GetInt(int minValue = 0, int maxValue = 606217)
    {
        return Random.Next(minValue, maxValue);
    }

    public static string GetString(string? prefix = null)
    {
        return prefix + '_' + Guid.NewGuid();
    }

    public static T GetEnum<T>() where T : struct, Enum
    {
        return Random.GetItems(Enum.GetValues<T>(), 1)[0];
    }
}
