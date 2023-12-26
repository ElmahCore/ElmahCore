using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ElmahCore.Mvc;

internal static class HttpContextExceptions
{
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
}