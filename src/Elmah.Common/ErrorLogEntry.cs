using System;

namespace Elmah;

/// <summary>
///     Binds an <see cref="Error" /> instance with the <see cref="ErrorLog" />
///     instance from where it was served.
/// </summary>
[Serializable]
public class ErrorLogEntry
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ErrorLogEntry" /> class
    ///     for a given unique error entry in an error log.
    /// </summary>
    public ErrorLogEntry(ErrorLog log, Error error)
    {
        Log = log ?? throw new ArgumentNullException(nameof(log));
        Error = error ?? throw new ArgumentNullException(nameof(error));
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

    public Guid Id => Error.Id;

    /// <summary>
    ///     Gets the <see cref="Error" /> object held in the entry.
    /// </summary>

    public Error Error { get; }
}