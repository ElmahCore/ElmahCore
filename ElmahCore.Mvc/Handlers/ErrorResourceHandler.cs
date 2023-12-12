using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;

namespace ElmahCore.Mvc.Handlers
{
    internal static partial class Endpoints
    {
        private static readonly HashSet<string> ResourceNames =
            new(typeof(ErrorLogMiddleware).GetTypeInfo().Assembly.GetManifestResourceNames(), StringComparer.OrdinalIgnoreCase);

        public static IEndpointConventionBuilder MapResources(this IEndpointRouteBuilder builder, string prefix = "")
        {
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            var assembly = typeof(Endpoints).Assembly;
            var resourcePrefix = $"{assembly.GetName().Name}.wwwroot.";
            var indexHtml = resourcePrefix + "index.html";

            return builder.Map($"{prefix}/{{*path}}", async ([FromRoute] string path, HttpContext context) =>
            {
                if (!path.Contains('.', StringComparison.Ordinal))
                {
                    using var stream2 = assembly.GetManifestResourceStream(indexHtml);
                    using var reader = new StreamReader(stream2 ?? throw new InvalidOperationException());

                    var elmahRoot = context.GetElmahRelativeRoot();
                    var html = await reader.ReadToEndAsync();
                    html = html.Replace("ELMAH_ROOT", elmahRoot);
                    return Results.Content(html, "text/html");
                }

                var resName = resourcePrefix + path.Replace('/', '.').Replace('\\', '.');
                if (!ResourceNames.Contains(resName))
                {
                    return Results.NotFound();
                }
                
                var resource = assembly.GetManifestResourceStream(resName);
                if (resource is null)
                {
                    return Results.NoContent();
                }

                contentTypeProvider.TryGetContentType(path, out string? contentType);
                return Results.Stream(resource, contentType);
            });
        }
    }
}