using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace Elmah.AspNetCore.Handlers;

internal static partial class Endpoints
{
    private static readonly HashSet<string> ResourceNames =
        new(typeof(Endpoints).Assembly.GetManifestResourceNames(), StringComparer.OrdinalIgnoreCase);

    private static readonly Assembly ThisAssembly = typeof(Endpoints).Assembly;
    private static readonly string ResourcePrefix = $"{ThisAssembly.GetName().Name}.wwwroot.";
    private static readonly string IndexHtml = ResourcePrefix + "index.html";

    private static async Task<IResult> ReturnIndex([FromServices] ILoggerFactory loggerFactory, HttpContext context)
    {
        using var stream = ThisAssembly.GetManifestResourceStream(IndexHtml);
        if (stream is null)
        {
            var logger = loggerFactory.CreateLogger("Elmah.AspNetCore");
            logger.LogError("{page} is not found for Elmah - has static content been generated?", IndexHtml);
            return Results.NotFound();
        }

        using var reader = new StreamReader(stream ?? throw new InvalidOperationException());

        var elmahRoot = context.GetElmahRelativeRoot();
        var html = await reader.ReadToEndAsync();
        html = html.Replace("ELMAH_ROOT", elmahRoot);
        return Results.Content(html, MediaTypeNames.Text.Html);
    }

    public static IEndpointConventionBuilder MapRoot(this IEndpointRouteBuilder builder, string prefix = "")
    {
        var handler = RequestDelegateFactory.Create(ReturnIndex);

        var pipeline = builder.CreateApplicationBuilder();
        pipeline.Run(handler.RequestDelegate);
        return builder.MapGet(prefix, pipeline.Build());
    }

    public static IEndpointConventionBuilder MapResources(this IEndpointRouteBuilder builder, string prefix = "")
    {
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        
        var handler = RequestDelegateFactory.Create(async ([FromRoute] string path, [FromServices] ILoggerFactory loggerFactory, HttpContext context) =>
        {
            if (!path.Contains('.', StringComparison.Ordinal))
            {
                return await ReturnIndex(loggerFactory, context);
            }

            var resName = ResourcePrefix + path.Replace('/', '.').Replace('\\', '.');
            if (!ResourceNames.Contains(resName))
            {
                return Results.NotFound();
            }
            
            var resource = ThisAssembly.GetManifestResourceStream(resName);
            if (resource is null)
            {
                return Results.NoContent();
            }

            contentTypeProvider.TryGetContentType(path, out string? contentType);
            return Results.Stream(resource, contentType);
        });

        var pipeline = builder.CreateApplicationBuilder();
        pipeline.Run(handler.RequestDelegate);
        return builder.Map($"{prefix}/{{*path}}", pipeline.Build());
    }
}