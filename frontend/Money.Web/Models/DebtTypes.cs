namespace Money.Web.Models;

public static class DebtTypes
{
    public static readonly Value None = new(0, "Неизвестный тип");

    public static Dictionary<int, Value> Values { get; } = GetValues();

    private static Dictionary<int, Value> GetValues()
    {
        return new[]
        {
            new Value(1, "Нужно забрать"),
            new Value(2, "Нужно отдать"),
        }.ToDictionary(x => x.Id, x => x);
    }

    public record Value(int Id, string Name);
}
