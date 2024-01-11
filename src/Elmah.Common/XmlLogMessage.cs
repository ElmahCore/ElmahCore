using System;
using Microsoft.Extensions.Logging;

namespace Elmah;

public sealed class XmlLogMessage : IElmahLogMessage
{
    public DateTime TimeStamp { get; set; }

    public string? Exception { get; set; }

    public string? Scope { get; set; }

    public LogLevel? Level { get; set; }

    public string? Message { get; set; }

    public string? Render() => this.Message;
}
