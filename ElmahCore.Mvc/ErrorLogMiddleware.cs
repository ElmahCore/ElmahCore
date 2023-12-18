using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ElmahCore.Mvc
{
    internal sealed class ErrorLogMiddleware
    {
        internal static bool ShowDebugPage = false;

        private readonly IElmahExceptionLogger _elmahLogger;
        private readonly bool _logRequestBody;
        private readonly RequestDelegate _next;
        
        public ErrorLogMiddleware(
            RequestDelegate next,
            IElmahExceptionLogger elmahLogger,
            IOptions<ElmahOptions> elmahOptions)
        {
            _next = next;
            _elmahLogger = elmahLogger;
            _logRequestBody = elmahOptions.Value.LogRequestBody;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? body = null;
            try
            {
                context.Features.Set<IElmahLogFeature>(new ElmahLogFeature());

                if (_logRequestBody)
                {
                    body = await context.ReadBodyAsync();
                }

                await _next(context);

                if (context.Response.HasStarted
                    || context.Response.StatusCode < 400
                    || context.Response.StatusCode >= 600
                    || context.Response.ContentLength.HasValue
                    || !string.IsNullOrEmpty(context.Response.ContentType))
                {
                    return;
                }

                await _elmahLogger.LogExceptionAsync(context, new HttpException(context.Response.StatusCode), body);
            }
            catch (Exception exception)
            {
                var entry = await _elmahLogger.LogExceptionAsync(context, exception, body);

                string? location = null;
                if (entry is not null)
                {
                    location = $"{context.GetElmahRelativeRoot()}/detail/{entry.Id}";
                    context.Features.Set<IElmahFeature>(new ElmahFeature(entry.Id, location));
                }

                //To next middleware
                if (entry is null || !ShowDebugPage)
                {
                    throw;
                }

                //Show Debug page
                context.Response.Redirect(location!);
            }
        }
    }
}