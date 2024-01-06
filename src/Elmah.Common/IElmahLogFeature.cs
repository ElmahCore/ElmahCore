using System;
using System.Collections.Generic;

namespace Elmah;

/// <summary>
/// Interface for object which is attached to <see cref="HttpContext"/> for use by Elmah.
/// </summary>
public interface IElmahLogFeature
{
    /// <summary>
    /// Gets the list of <see cref="ElmahLogMessageEntry"/> instances.
    /// </summary>
    IReadOnlyCollection<IElmahLogMessage> Log { get; }
    
    /// <summary>
    /// Gets the list of <see cref="ElmahLogParameters"/> instances.
    /// </summary>
    IReadOnlyCollection<ElmahLogParameters> Params { get; }

    /// <summary>
    /// Gets the list of <see cref="ElmahLogSqlEntry"/> instances.
    /// </summary>
    IReadOnlyCollection<ElmahLogSqlEntry> LogSql { get; }

    /// <summary>
    /// Logs the parameters for Elmah.
    /// </summary>
    /// <param name="list">The list of key value pairs</param>
    /// <param name="typeName">The type name</param>
    /// <param name="memberName">The member name</param>
    /// <param name="file">The file name</param>
    /// <param name="line">The line number</param>
    void LogParameters((string name, object? value)[] list, string typeName, string memberName, string file, int line);

    void SetSqlDuration(Guid id);

    void AddSql(Guid id, ElmahLogSqlEntry entry);

    void AddMessage(IElmahLogMessage entry);
}