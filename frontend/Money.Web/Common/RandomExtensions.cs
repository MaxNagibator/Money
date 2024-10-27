namespace Money.Web.Common;

public static class RandomExtensions
{
    public static string NextColor(this Random random)
    {
        int red = random.Next(0, 256);
        int green = random.Next(0, 256);
        int blue = random.Next(0, 256);

        return $"#{red:X2}{green:X2}{blue:X2}FF";
    }
}
