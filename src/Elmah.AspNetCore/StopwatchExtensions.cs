using System;
using System.Diagnostics;

namespace Elmah.AspNetCore;

internal static class StopwatchExtensions
{
#if NET6_0
    private const long TicksPerMillisecond = 10000;
    private const long TicksPerSecond = TicksPerMillisecond * 1000;
    private static readonly double TickFrequency = (double)TicksPerSecond / Stopwatch.Frequency;

    public static TimeSpan GetElapsedTime(long startingTimestamp)
    {
        long endingTimestamp = Stopwatch.GetTimestamp();
        return new TimeSpan((long)((endingTimestamp - startingTimestamp) * TickFrequency));
    }
#else
    public static TimeSpan GetElapsedTime(long startingTimestamp) => Stopwatch.GetElapsedTime(startingTimestamp);
#endif
}
