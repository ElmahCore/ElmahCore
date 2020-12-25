using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ElmahCore.Mvc.Handlers
{
    /// <summary>
    /// Renders an error as JSON Text (RFC 4627).
    /// </summary>

    static class ErrorJsonHandler
    {
        public static async Task ProcessRequest(HttpContext context, ErrorLog errorLog)
        {
            var response = context.Response;
            response.ContentType = "application/json";

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
            // Stream out the error as formatted JSON.
            //
            var jsonSerializerOptions = new JsonSerializerOptions {IgnoreNullValues = true};
            var err = new ErrorWrapper(entry?.Error) {HtmlMessage = null};
            var jsonString = JsonSerializer.Serialize(err,jsonSerializerOptions);
            await response.WriteAsync(jsonString);
        }
    }
}