namespace Money.Web.Common;

public static class RandomExtensions
{
    public static string NextColor(this Random random)
    {
        var red = random.Next(0, 256);
        var green = random.Next(0, 256);
        var blue = random.Next(0, 256);

        return $"#{red:X2}{green:X2}{blue:X2}FF";
    }
}
