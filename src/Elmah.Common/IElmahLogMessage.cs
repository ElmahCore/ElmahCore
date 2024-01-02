using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Elmah;

public interface IElmahLogMessage
{
    DateTime TimeStamp { get; }
    string? Exception { get; }
    string? Scope { get; }
    LogLevel? Level { get; }
    string? Message => this.Render();
    string? Render();
    KeyValuePair<string, string>[]? Params { get; }
    bool Collapsed => true;
}