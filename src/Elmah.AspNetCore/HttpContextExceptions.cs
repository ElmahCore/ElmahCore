using System;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Elmah.AspNetCore;

internal static class HttpContextExceptions
{
    private static readonly MediaTypeHeaderValue TextHtmlMediaType = new(MediaTypeNames.Text.Html);

    public static string GetElmahRelativeRoot(this HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<ElmahEnvironment>();
        return context.Request.PathBase.Add(options.Path);
    }

    public static string GetElmahAbsoluteRoot(this HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<ElmahEnvironment>();
        return $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase.Add(options.Path)}";
    }

    public static bool RequestAcceptsHtml(this HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            return false;
        }

        if ("XMLHttpRequest".Equals(context.Request.Headers.XRequestedWith, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var headers = context.Request.GetTypedHeaders();
        return headers.Accept?.Any(x => x.IsSubsetOf(TextHtmlMediaType)) ?? false;
    }
}