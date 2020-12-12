using System;
using System.Threading.Tasks;
using ElmahCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace ElmahCore
{
    public static class ElmahExtensions
    {
        internal static ErrorLogMiddleware LogMiddleware;
        public static void RiseError(this HttpContext ctx, Exception ex, Func<HttpContext, Error, Task> onError)
        {
            var middleware = ctx.RequestServices.GetService<ErrorLogMiddleware>();
            middleware?.LogException(ex, ctx, onError);
        }
        public static void RiseError(this HttpContext ctx, Exception ex)
        {
            var middleware = ctx.RequestServices.GetService<ErrorLogMiddleware>();
            middleware?.LogException(ex,ctx, (context, error) => Task.CompletedTask);
        }
        public static void RiseError(Exception ex)
        {
            LogMiddleware?.LogException(ex, new DefaultHttpContext(), (context, error) => Task.CompletedTask);
        }
    }
}