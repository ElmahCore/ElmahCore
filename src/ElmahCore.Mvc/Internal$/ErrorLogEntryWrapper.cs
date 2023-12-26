using System;

namespace ElmahCore.Mvc;

[Serializable]
internal class ErrorLogEntryWrapper
{
    public ErrorLogEntryWrapper(ErrorLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        Log = entry.Log;
        Id = entry.Id;
        Error = new ErrorWrapper(entry.Error, entry.Log.SourcePaths);
    }

    /// <summary>
    ///     Gets the <see cref="ErrorLog" /> instance where this entry
    ///     originated from.
    /// </summary>

    public ErrorLog Log { get; }

    /// <summary>
    ///     Gets the unique identifier that identifies the error entry
    ///     in the log.
    /// </summary>

    public Guid Id { get; }

    /// <summary>
    ///     Gets the <see cref="Error" /> object held in the entry.
    /// </summary>

    public ErrorWrapper Error { get; }
}