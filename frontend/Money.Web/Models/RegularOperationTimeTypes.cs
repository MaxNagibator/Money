namespace Money.Web.Models;

public static class RegularOperationTimeTypes
{
    public static readonly Value None = new(0, "Неизвестный тип");

    public static Dictionary<int, Value> Values { get; } = GetValues();

    private static Dictionary<int, Value> GetValues()
    {
        return new[]
        {
            new Value(1, "Каждый день"),
            new Value(2, "Каждую неделю"),
            new Value(3, "Каждый месяц"),
        }.ToDictionary(x => x.Id, x => x);
    }

    public record Value(int Id, string Name);
}
