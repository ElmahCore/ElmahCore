using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ElmahCore.Mvc;

internal sealed class ErrorLogMiddleware
{
    private readonly IElmahExceptionLogger _elmahLogger;
    private readonly bool _logRequestBody;
    private readonly bool _showDebugPage;
    private readonly RequestDelegate _next;
    
    public ErrorLogMiddleware(
        RequestDelegate next,
        IElmahExceptionLogger elmahLogger,
        IOptions<ElmahOptions> elmahOptions)
    {
        _next = next;
        _elmahLogger = elmahLogger;
        _logRequestBody = elmahOptions.Value.LogRequestBody;
        _showDebugPage = elmahOptions.Value.LogRequestBody;
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
        string? body = await this.GetBodyAsync(context);
        var entry = await _elmahLogger.LogExceptionAsync(context, exceptionInfo.SourceException, body);

        string? location = null;
        if (entry is not null)
        {
            location = $"{context.GetElmahRelativeRoot()}/detail/{entry.Id}";
            context.Features.Set<IElmahFeature>(new ElmahFeature(entry.Id, location));
        }

        //To next middleware
        if (entry is null || !_showDebugPage)
        {
            exceptionInfo.Throw();
            return;
        }

        //Show Debug page
        context.Response.Redirect(location!);
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

        string? body = await this.GetBodyAsync(context);
        await _elmahLogger.LogExceptionAsync(context, new HttpException(context.Response.StatusCode), body);
    }

    private Task<string?> GetBodyAsync(HttpContext context)
    {
        if (_logRequestBody)
        {
            return context.ReadBodyAsync();
        }

        return Task.FromResult<string?>(null);
    }
}