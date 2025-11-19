namespace Money.Web.Common;

public static class ChartColors
{
    public static readonly List<string> All =
    [
        Colors.Blue.Default,
        Colors.Red.Default,
        Colors.Green.Default,
        Colors.Amber.Default,
        Colors.Purple.Default,
        Colors.Cyan.Default,
        Colors.Teal.Default,
        Colors.DeepOrange.Default,
        Colors.Pink.Default,
        Colors.Indigo.Default,
        Colors.LightBlue.Default,
        Colors.Lime.Default,
        Colors.Yellow.Default,
        Colors.Orange.Default,
        Colors.DeepPurple.Default,
        Colors.Brown.Default,
        Colors.BlueGray.Default,
        Colors.Gray.Default,
    ];

    public static string GetColor(int index)
    {
        return All[index % All.Count];
    }
}
