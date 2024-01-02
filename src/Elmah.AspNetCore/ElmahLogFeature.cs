using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Elmah.AspNetCore;

internal class ElmahLogFeature : IElmahLogFeature
{
    private readonly ConcurrentDictionary<Guid, ElmahLogSqlEntry> _map = new();
    private readonly ConcurrentBag<IElmahLogMessage> _logs = new();
    private readonly ConcurrentBag<ElmahLogParameters> _params = new();

    public IReadOnlyCollection<IElmahLogMessage> Log => _logs.ToList();
    public IReadOnlyCollection<ElmahLogParameters> Params => _params.ToList();
    public IReadOnlyCollection<ElmahLogSqlEntry> LogSql => _map.Values.OrderBy(i => i.TimeStamp).ToList();

    public void AddMessage(IElmahLogMessage entry)
    {
        _logs.Add(entry);
    }

    public void AddSql(Guid id, ElmahLogSqlEntry entry)
    {
        _map.TryAdd(id, entry);
    }

    public void SetSqlDuration(Guid id)
    {
        if (_map.TryGetValue(id, out ElmahLogSqlEntry? data))
        {
            data.DurationMs = StopwatchExtensions.GetElapsedTime(data.TimerStart).TotalMilliseconds;
        }
    }

    public void LogParameters((string name, object value)[] list, string typeName, string memberName,
        string file, int line)
    {
        _params.Add(new ElmahLogParameters(DateTime.Now, list, typeName, memberName, file, line));
    }
}
