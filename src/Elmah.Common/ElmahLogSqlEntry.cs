using System;

namespace Elmah;

public class ElmahLogSqlEntry
{
    public DateTime TimeStamp { get; set; }
    public long TimerStart { get; set; }
    public string? SqlText { get; set; }
    public string? CommandType { get; set; }
    public double DurationMs { get; set; }
}