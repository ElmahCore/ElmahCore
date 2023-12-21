using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;

namespace ElmahCore.Mvc;

internal static class HttpContextExceptions
{
    private static readonly string[] SupportedContentTypes =
{
        MediaTypeNames.Application.Json,
        "application/x-www-form-urlencoded",
        "application/javascript",
        MediaTypeNames.Application.Soap,
        "application/xhtml+xml",
        MediaTypeNames.Application.Xml,
        MediaTypeNames.Text.Html,
        "text/javascript",
        MediaTypeNames.Text.Plain,
        MediaTypeNames.Text.Xml,
        "text/markdown"
    };

    public static string GetElmahRelativeRoot(this HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<IOptions<ElmahOptions>>().Value;
        return context.Request.PathBase.Add(options.Path);
    }

    public static string GetElmahAbsoluteRoot(this HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<IOptions<ElmahOptions>>().Value;
        return $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase.Add(options.Path)}";
    }

    public static async Task<string?> ReadBodyAsync(this HttpContext context)
    {
        var ct = context.Request.ContentType?.ToLower();
        var tEnc = string.Join(",", context.Request.Headers[HeaderNames.TransferEncoding].ToArray());
        if (string.IsNullOrEmpty(ct) || tEnc.Contains("chunked") || !SupportedContentTypes.Any(i => ct.Contains(ct)))
        {
            return null;
        }

        context.Request.EnableBuffering();
        var body = context.Request.Body;
        var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];

        // ReSharper disable once MustUseReturnValue
        await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
        var bodyAsText = Encoding.UTF8.GetString(buffer);
        body.Seek(0, SeekOrigin.Begin);
        context.Request.Body = body;

        return bodyAsText;
    }
}