using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Elmah;

public static class ErrorLogExtensions
{
    /// <summary>
    ///     When overridden in a subclass, starts a task that asynchronously
    ///     does the same as <see cref="Log(Error)" />. An additional parameter
    ///     specifies a <see cref="CancellationToken" /> to use.
    /// </summary>
    public static Task LogAsync(this ErrorLog log, Error error) => log.LogAsync(error, default);

    /// <summary>
    ///     When overridden in a subclass, starts a task that asynchronously
    ///     does the same as <see cref="GetError" />.
    /// </summary>
    public static Task<ErrorLogEntry?> GetErrorAsync(this ErrorLog log, Guid id) => log.GetErrorAsync(id, default);

    /// <summary>
    ///     When overridden in a subclass, starts a task that asynchronously
    ///     does the same as <see cref="GetErrors" />. An additional
    ///     parameter specifies a <see cref="CancellationToken" /> to use.
    /// </summary>
    public static Task<int> GetErrorsAsync(this ErrorLog log, ErrorLogFilterCollection errorLogFilters, int errorIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList)
        => log.GetErrorsAsync(errorLogFilters, errorIndex, pageSize, errorEntryList, default);
}
