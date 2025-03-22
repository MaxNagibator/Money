using System.Diagnostics;

namespace Money.Common;

public readonly struct ValueStopwatch
{
    private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
    private readonly long _startTimestamp;

    private ValueStopwatch(long startTimestamp)
    {
        _startTimestamp = startTimestamp;
    }

    public static ValueStopwatch StartNew()
    {
        return new(Stopwatch.GetTimestamp());
    }

    public TimeSpan GetElapsedTime()
    {
        if (_startTimestamp == 0)
        {
            throw new InvalidOperationException("Stopwatch не был запущен");
        }

        var end = Stopwatch.GetTimestamp();
        var timestampDelta = end - _startTimestamp;
        var ticks = (long)(TimestampToTicks * timestampDelta);
        return new(ticks);
    }
}
