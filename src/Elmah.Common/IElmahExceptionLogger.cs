using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace Elmah;

public interface IElmahExceptionLogger
{
    /// <summary>
    /// Adds the provided exception into the backing store.
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="exception">The exception</param>
    /// <returns>The <see cref="ErrorLogEntry"/> for the created log entry</returns>
    Task<ErrorLogEntry?> LogExceptionAsync(HttpContext context, Exception exception);
}