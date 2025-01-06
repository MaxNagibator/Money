namespace Money.Web.Models;

public static class OperationTypes
{
    public static readonly Value None = new(0, "Неизвестный тип", Icons.Material.Rounded.Error, Color.Error);

    public static Value[] Values { get; } = GetValues();

    private static Value[] GetValues()
    {
        return
        [
            new(1, "Расходы", Icons.Material.Rounded.ArrowCircleDown, Color.Warning),
            new(2, "Доходы", Icons.Material.Rounded.ArrowCircleUp, Color.Success),
        ];
    }

    public record Value(int Id, string Name, string Icon, Color Color)
    {
        public string AddText { get; } = "Добавить " + Name.ToLower();
    }
}
