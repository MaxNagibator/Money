namespace Money.Web.Models;

public static class PaymentTypes
{
    public static readonly Value None = new(0, "Неизвестный тип", Icons.Material.Rounded.Error, Color.Error);

    public static Value[] Values { get; } = GetValues();

    private static Value[] GetValues()
    {
        return
        [
            new Value(1, "Расходы", Icons.Material.Rounded.ArrowCircleDown, Color.Warning),
            new Value(2, "Доходы", Icons.Material.Rounded.ArrowCircleUp, Color.Success),
        ];
    }

    public record Value(int Id, string Name, string Icon, Color Color);
}
