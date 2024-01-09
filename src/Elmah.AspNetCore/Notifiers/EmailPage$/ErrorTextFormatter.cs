using System.IO;

namespace Elmah.AspNetCore.Notifiers;

/// <summary>
///     Provides the base contract for implementations that render
///     text-based formatting for an error.
/// </summary>
internal abstract class ErrorTextFormatter
{
    /// <summary>
    ///     Gets the MIME type of the text format provided by the formatter
    ///     implementation.
    /// </summary>
    public abstract string MimeType { get; }

    /// <summary>
    ///     Formats a text representation of the given <see cref="Error" />
    ///     instance using a <see cref="TextWriter" />.
    /// </summary>
    public abstract void Format(TextWriter writer, Error error);
}