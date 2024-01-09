using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Elmah;

public interface IErrorFactory
{
    /// <summary>
    /// Creates an <see cref="Error"/> instance with context added from request.
    /// </summary>
    /// <param name="exception">The exception</param>
    /// <param name="context">The HTTP context</param>
    /// <returns>The <see cref="Error"/> instance</returns>
    Task<Error> CreateAsync(Exception exception, HttpContext? context);
}
