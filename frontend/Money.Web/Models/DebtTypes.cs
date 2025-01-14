namespace Money.Web.Models;

public static class DebtTypes
{
    public static readonly Value None = new(0, "Неизвестный тип");

    public static Dictionary<int, Value> Values { get; } = GetValues();

    private static Dictionary<int, Value> GetValues()
    {
        return new[]
        {
            new Value(1, "Нужно забрать", "Забрать своё"),
            new Value(2, "Нужно отдать", "Отдать чужое"),
        }.ToDictionary(x => x.Id, x => x);
    }

    public record Value(int Id, string Name, string? AddText = null)
    {
        public string AddText { get; } = AddText ?? "Добавить";
    }
}
