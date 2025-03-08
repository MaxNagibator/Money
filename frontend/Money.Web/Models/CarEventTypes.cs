namespace Money.Web.Models;

public static class CarEventTypes
{
    public static readonly Value None = new(0, "Неизвестный тип", "");

    private const string IconsPath = "car_events/";

    public static Dictionary<int, Value> Values { get; } = GetValues();

    private static Dictionary<int, Value> GetValues()
    {
        return new[]
        {
            new Value(1, "Приобретение", "sdelka_64.png"),
            new Value(2, "Страховка", "strahovka_64.png"),
            new Value(3, "Обслуживание", "service_64.png"),
            new Value(4, "Переобувка", "pereobyvka_64.png"),
            new Value(99, "Прочее", "other_64.png"),
        }.ToDictionary(x => x.Id, x => x);
    }

    public record Value(int Id, string Name, string Icon)
    {
        public string GetIcon()
        {
            return IconsPath + Icon;
        }
    }
}
