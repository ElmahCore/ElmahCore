using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elmah.AspNetCore.Handlers;

/// <summary>
///     Renders an error as JSON Text (RFC 4627).
/// </summary>
internal static partial class Endpoints
{
    public static IEndpointConventionBuilder MapJson(this IEndpointRouteBuilder builder, string prefix = "")
    {
        var handler = RequestDelegateFactory.Create(async ([FromQuery] string id, [FromServices] ErrorLog errorLog) =>
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid errorGuid))
            {
                throw new ApplicationException("Missing error identifier specification.");
            }

            var entry = await errorLog.GetErrorAsync(errorGuid);

            //
            // Perhaps the error has been deleted from the store? Whatever
            // the reason, pretend it does not exist.
            //
            if (entry == null)
            {
                return Results.NotFound();
            }

            // 
            // Stream out the error as formatted JSON.
            //
            var err = new ErrorWrapper(entry.Error, errorLog.SourcePaths) { HtmlMessage = null };
            return Results.Json(err, DefaultJsonSerializerOptions.IgnoreDefault);
        });

        var pipeline = builder.CreateApplicationBuilder();
        pipeline.Run(handler.RequestDelegate);
        return builder.MapMethods($"{prefix}/json", new[] { HttpMethods.Get, HttpMethods.Post }, pipeline.Build());
    }
}