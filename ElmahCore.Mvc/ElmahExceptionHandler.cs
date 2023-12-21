#if USE_GLOBAL_ERROR_HANDLING
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ElmahCore.Mvc;

internal class ElmahExceptionHandler : IExceptionHandler
{
    private readonly IElmahExceptionLogger _elmahLogger;
    private readonly ElmahOptions _elmahOptions;

    public ElmahExceptionHandler(IElmahExceptionLogger elmahLogger, IOptions<ElmahOptions> elmahOptions)
    {
        _elmahLogger = elmahLogger;
        _elmahOptions = elmahOptions.Value;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        string? body = null;
        if (_elmahOptions.LogRequestBody)
        {
            body = await httpContext.ReadBodyAsync();
        }

        var entry = await _elmahLogger.LogExceptionAsync(httpContext, exception, body);

        string? location = null;
        if (entry is not null)
        {
            location = $"{httpContext.GetElmahRelativeRoot()}/detail/{entry.Id}";
            httpContext.Features.Set<IElmahFeature>(new ElmahFeature(entry.Id, location));
        }

        if (!string.IsNullOrEmpty(location) && _elmahOptions.ShowElmahErrorPage)
        {
            httpContext.Response.Redirect(location);
            return true;
        }

        return false;
    }
}
#endif