using System;
using System.Runtime.CompilerServices;
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
            middleware?.LogException(ex, ctx, (context, error) => Task.CompletedTask);
        }

        public static void RiseError(Exception ex)
        {
            LogMiddleware?.LogException(ex, InternalHttpContext.Current ?? new DefaultHttpContext(),
                (context, error) => Task.CompletedTask);
        }

        public static void LogParams(this object source,
            (string name, object value) param1 = default,
            (string name, object value) param2 = default,
            (string name, object value) param3 = default,
            (string name, object value) param4 = default,
            (string name, object value) param5 = default,
            (string name, object value) param6 = default,
            (string name, object value) param7 = default,
            (string name, object value) param8 = default,
            (string name, object value) param9 = default,
            (string name, object value) param10 = default,
            [CallerMemberName] string memberName = null,
            [CallerFilePath] string file = null,
            [CallerLineNumber] int line = 0)
        {
            try
            {
                var feature = InternalHttpContext.Current.Features.Get<ElmahLogFeature>();
                if (feature == null) return;

                var list = new[] {param1, param2, param3, param4, param5, param6, param7, param8, param9, param10};

                var typeName = source.GetType().ToString();
                feature.LogParameters(list, typeName, memberName, file, line);

            }
            catch
            {
                //ignored
            }
        }
    }
}