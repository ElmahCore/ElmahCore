using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Elmah;

public sealed class XmlLogMessage : IElmahLogMessage
{
    public DateTime TimeStamp { get; set; }

    public string? Exception { get; set; }

    public string? Scope { get; set; }

    public LogLevel? Level { get; set; }

    public string? Message { get; set; }

    public KeyValuePair<string, string>[] Params { get; set; } = Array.Empty<KeyValuePair<string, string>>();

    public string? Render() => this.Message;
}
