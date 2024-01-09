using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Elmah;

public sealed class ElmahLogMessageEntry<TState> : IElmahLogMessage
{
    public DateTime TimeStamp { get; init; }
    public string? Scope { get; init; }
    public Exception? Exception { get; init; }
    string? IElmahLogMessage.Exception => this.Exception?.ToString();
    public LogLevel? Level { get; init; }
    public TState State { get; init; } = default!;
    public Func<TState, Exception?, string> Formatter { get; init; } = default!;
    public string Render() => this.Formatter(this.State, this.Exception);
}
