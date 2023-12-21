using System;

namespace ElmahCore;

public class ElmahLogSqlEntry
{
    public DateTime TimeStamp { get; set; }
    public string? SqlText { get; set; }
    public string? CommandType { get; set; }
    public int DurationMs { get; set; }
}