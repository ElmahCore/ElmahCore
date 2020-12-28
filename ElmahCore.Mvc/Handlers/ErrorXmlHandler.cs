using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;

namespace ElmahCore.Mvc.Handlers
{

    /// <summary>
    /// Renders an error as an XML document.
    /// </summary>

    static class ErrorXmlHandler
    {
        public static async Task ProcessRequest(HttpContext context, ErrorLog errorLog)
        {
            var response = context.Response;
            response.ContentType = "application/xml";

            //
            // Retrieve the ID of the requested error and read it from 
            // the store.
            //

            var errorId = context.Request.Query["id"].FirstOrDefault();

            if (string.IsNullOrEmpty(errorId))
                throw new ApplicationException("Missing error identifier specification.");

            var entry = await errorLog.GetErrorAsync(errorId);

            //
            // Perhaps the error has been deleted from the store? Whatever
            // the reason, pretend it does not exist.
            //

            if (entry == null)
            {
                context.Response.StatusCode = 404;
            }

            //
            // Stream out the error as formatted XML.
            //

            var wrappedError = new ErrorWrapper(entry?.Error, errorLog.SourcePaths);
            var xmlSerializer = new XmlSerializer(wrappedError.GetType(), new XmlRootAttribute("Error"));
            using(var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, wrappedError);
                await response.WriteAsync(textWriter.ToString(), Encoding.UTF8);
            }

        }
    }

}