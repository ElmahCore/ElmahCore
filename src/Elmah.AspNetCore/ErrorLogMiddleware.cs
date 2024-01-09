using System;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Elmah.AspNetCore;

internal sealed class ErrorLogMiddleware
{
    private readonly IElmahExceptionLogger _elmahLogger;
    private readonly IOptions<ElmahOptions> _options;
    private readonly RequestDelegate _next;
    
    public ErrorLogMiddleware(
        RequestDelegate next,
        IElmahExceptionLogger elmahLogger,
        IOptions<ElmahOptions> elmahOptions)
    {
        _next = next;
        _elmahLogger = elmahLogger;
        _options = elmahOptions;
    }

    public Task InvokeAsync(HttpContext context)
    {
#if USE_GLOBAL_ERROR_HANDLING
        // Dotnet 8+ we will use built-in exception middleware, but need to handle cases with error status codes
        // and attach feature for consumers to access.
        return this.ExecuteMiddlewareAsync(context);
#else
        ExceptionDispatchInfo exceptionInfo;
        try
        {
            var task = this.ExecuteMiddlewareAsync(context);
            if (!task.IsCompletedSuccessfully)
            {
                return Awaited(this, context, task);
            }

            return task;
        }
        catch (Exception exception)
        {
            exceptionInfo = ExceptionDispatchInfo.Capture(exception);
        }

        return this.HandleExceptionAsync(context, exceptionInfo);

        async Task Awaited(ErrorLogMiddleware middleware, HttpContext context, Task task)
        {
            ExceptionDispatchInfo? exceptionInfo = null;
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            }

            if (exceptionInfo is not null)
            {
                await middleware.HandleExceptionAsync(context, exceptionInfo);
            }
        }
#endif
    }

    private async Task HandleExceptionAsync(HttpContext context, ExceptionDispatchInfo exceptionInfo)
    {
        var entry = await _elmahLogger.LogExceptionAsync(context, exceptionInfo.SourceException);

        string? location = null;
        if (entry is not null)
        {
            location = $"{context.GetElmahRelativeRoot()}/detail/{entry.Id}";
            context.Features.Set<IElmahFeature>(new ElmahFeature(entry.Id, location));
        }

        //To next middleware
        if (entry is null || !_options.Value.ShowElmahErrorPage)
        {
            exceptionInfo.Throw();
            return;
        }

        //Show Debug page
        context.Response.StatusCode = ErrorFactory.GetStatusCodeFromExceptionOr500(exceptionInfo.SourceException);
        if (context.RequestAcceptsHtml())
        {
            context.Response.Redirect(location!);
        }
    }

    private async Task ExecuteMiddlewareAsync(HttpContext context)
    {
        context.Features.Set<IElmahLogFeature>(new ElmahLogFeature());

        await _next(context);

        if (context.Response.HasStarted
            || context.Response.StatusCode < 400
            || context.Response.StatusCode >= 600
            || context.Response.ContentLength.HasValue
            || !string.IsNullOrEmpty(context.Response.ContentType))
        {
            return;
        }

        var exception = new BadHttpRequestException("An error status was returned when processing the request", context.Response.StatusCode);
        await _elmahLogger.LogExceptionAsync(context, exception);
    }
}