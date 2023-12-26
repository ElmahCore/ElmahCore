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
    /// <param name="context">The HTTP context</param>
    /// <param name="exeption">The exception</param>
    /// <returns>The <see cref="ErrorLogEntry"/> for the created log entry</returns>
    Task<ErrorLogEntry?> LogExceptionAsync(HttpContext context, Exception exeption);
}