using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ElmahCore.Mvc.Handlers;

/// <summary>
///     Renders an error as an XML document.
/// </summary>
internal static partial class Endpoints
{
    public static IEndpointConventionBuilder MapXml(this IEndpointRouteBuilder builder, string prefix = "")
    {
        return builder.MapGet($"{prefix}/xml", async ([FromQuery] string id, [FromServices] ErrorLog errorLog) =>
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
            // Stream out the error as formatted XML.
            //
            var wrappedError = new ErrorWrapper(entry.Error, errorLog.SourcePaths);
            var xmlSerializer = new XmlSerializer(wrappedError.GetType(), new XmlRootAttribute("Error"));

            using var textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter, wrappedError);
            return Results.Content(textWriter.ToString(), MediaTypeNames.Application.Xml, Encoding.UTF8);
        });
    }
}