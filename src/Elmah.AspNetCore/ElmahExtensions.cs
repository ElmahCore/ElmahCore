using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Elmah.AspNetCore;

public static class ElmahExtensions
{
    public static Task RaiseError(this HttpContext context, Exception exception)
    {
        var logger = context.RequestServices.GetRequiredService<IElmahExceptionLogger>();
        return logger.LogExceptionAsync(context, exception);
    }

    public static void LogParams(this object source,
        HttpContext ctx,
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
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? file = null,
        [CallerLineNumber] int line = 0)
    {
        try
        {
            var feature = ctx.Features.Get<IElmahLogFeature>();
            if (feature is null)
            {
                return;
            }

            var list = new[] { param1, param2, param3, param4, param5, param6, param7, param8, param9, param10 };

            var typeName = source.GetType().ToString();
            feature.LogParameters(list, typeName, memberName!, file!, line);
        }
        catch
        {
            //ignored
        }
    }
}