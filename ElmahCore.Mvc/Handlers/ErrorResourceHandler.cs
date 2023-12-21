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

namespace ElmahCore.Mvc.Handlers
{
    internal static partial class Endpoints
    {
        private static readonly HashSet<string> ResourceNames =
            new(typeof(ErrorLogMiddleware).GetTypeInfo().Assembly.GetManifestResourceNames(), StringComparer.OrdinalIgnoreCase);

        private static readonly Assembly ThisAssembly = typeof(Endpoints).Assembly;
        private static readonly string ResourcePrefix = $"{ThisAssembly.GetName().Name}.wwwroot.";
        private static readonly string IndexHtml = ResourcePrefix + "index.html";

        private static async Task<IResult> ReturnIndex(HttpContext context)
        {
            using var stream = ThisAssembly.GetManifestResourceStream(IndexHtml);
            using var reader = new StreamReader(stream ?? throw new InvalidOperationException());

            var elmahRoot = context.GetElmahRelativeRoot();
            var html = await reader.ReadToEndAsync();
            html = html.Replace("ELMAH_ROOT", elmahRoot);
            return Results.Content(html, MediaTypeNames.Text.Html);
        }

        public static IEndpointConventionBuilder MapRoot(this IEndpointRouteBuilder builder, string prefix = "")
        {
            return builder.MapGet(prefix, (Delegate)ReturnIndex);
        }

        public static IEndpointConventionBuilder MapResources(this IEndpointRouteBuilder builder, string prefix = "")
        {
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            
            return builder.Map($"{prefix}/{{*path}}", async ([FromRoute] string path, HttpContext context) =>
            {
                if (!path.Contains('.', StringComparison.Ordinal))
                {
                    return await ReturnIndex(context);
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
        }
    }
}