using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace ElmahCore;

public interface IElmahExceptionLogger
{
    /// <summary>
    /// Adds the provided exception into the backing store.
    /// </summary>
    /// <param name="ctx">The HTTP context</param>
    /// <param name="ex">The exception</param>
    /// <param name="onError">The onError delegate</param>
    /// <param name="body">The HTTP response body (optional)</param>
    /// <returns>The <see cref="ErrorLogEntry"/> for the created log entry</returns>
    Task<ErrorLogEntry?> LogExceptionAsync(HttpContext ctx, Exception ex, string? body = null);
}