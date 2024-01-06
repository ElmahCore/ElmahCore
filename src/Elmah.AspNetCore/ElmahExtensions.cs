using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Elmah.AspNetCore;

public static class ElmahExtensions
{
    public static Task RaiseErrorAsync(this HttpContext context, Exception exception)
    {
        var logger = context.RequestServices.GetRequiredService<IElmahExceptionLogger>();
        return logger.LogExceptionAsync(context, exception);
    }

    public static void LogParamsToElmah(
        this HttpContext ctx,
        object source,
        object? param1Value = default,
        object? param2Value = default,
        object? param3Value = default,
        object? param4Value = default,
        object? param5Value = default,
        object? param6Value = default,
        object? param7Value = default,
        object? param8Value = default,
        object? param9Value = default,
        object? param10Value = default,
        [CallerArgumentExpression(nameof(param1Value))] string param1Name = "",
        [CallerArgumentExpression(nameof(param2Value))] string param2Name = "",
        [CallerArgumentExpression(nameof(param3Value))] string param3Name = "",
        [CallerArgumentExpression(nameof(param4Value))] string param4Name = "",
        [CallerArgumentExpression(nameof(param5Value))] string param5Name = "",
        [CallerArgumentExpression(nameof(param6Value))] string param6Name = "",
        [CallerArgumentExpression(nameof(param7Value))] string param7Name = "",
        [CallerArgumentExpression(nameof(param8Value))] string param8Name = "",
        [CallerArgumentExpression(nameof(param9Value))] string param9Name = "",
        [CallerArgumentExpression(nameof(param10Value))] string param10Name = "",
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

            var list = new List<(string, object?)>(10);
            if (!string.IsNullOrEmpty(param1Name))
            {
                list.Add((param1Name, param1Value));
            }

            if (!string.IsNullOrEmpty(param2Name))
            {
                list.Add((param2Name, param2Value));
            }

            if (!string.IsNullOrEmpty(param3Name))
            {
                list.Add((param3Name, param3Value));
            }

            if (!string.IsNullOrEmpty(param4Name))
            {
                list.Add((param4Name, param4Value));
            }

            if (!string.IsNullOrEmpty(param5Name))
            {
                list.Add((param5Name, param5Value));
            }

            if (!string.IsNullOrEmpty(param6Name))
            {
                list.Add((param6Name, param6Value));
            }

            if (!string.IsNullOrEmpty(param7Name))
            {
                list.Add((param7Name, param7Value));
            }

            if (!string.IsNullOrEmpty(param8Name))
            {
                list.Add((param8Name, param8Value));
            }

            if (!string.IsNullOrEmpty(param9Name))
            {
                list.Add((param9Name, param9Value));
            }

            if (!string.IsNullOrEmpty(param10Name))
            {
                list.Add((param10Name, param10Value));
            }

            var typeName = source.GetType().ToString();
            feature.LogParameters(list.ToArray(), typeName, memberName!, file!, line);
        }
        catch
        {
            // ignored
        }
    }
}