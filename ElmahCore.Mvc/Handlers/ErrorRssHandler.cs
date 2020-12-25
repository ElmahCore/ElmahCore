using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElmahCore.Mvc.Xml;
using Microsoft.AspNetCore.Http;

namespace ElmahCore.Mvc.Handlers
{
    /// <summary>
    /// Renders a XML using the RSS 0.91 vocabulary that displays, at most,
    /// the 15 most recent errors recorded in the error log.
    /// </summary>

    static class ErrorRssHandler
    {
        public static async Task ProcessRequest(HttpContext context, ErrorLog errorLog, string elmahRoot)
        {
            const int pageSize = 15;
            var entries = new List<ErrorLogEntry>(pageSize);
            var log = errorLog;
            await log.GetErrorsAsync(0, pageSize, entries);

            var response = context.Response;
            response.ContentType = "application/xml";

            var title = $@"Error log of {log.ApplicationName} on {Environment.MachineName}";


            var link = $"{context.Request.Scheme}://{context.Request.Host}{elmahRoot}";
            var baseUrl = new Uri(link.TrimEnd('/') + "/");

            var items =
                from entry in entries
                let error = entry.Error
                select RssXml.Item(
                    error.Message,
                    "An error of type " + error.Type + " occurred. " + error.Message,
                    error.Time,
                    baseUrl + "detail?id=" + Uri.EscapeDataString(entry.Id));
            
            var rss = RssXml.Rss(title, link, "AddMessage of recent errors", items);

            await response.WriteAsync(XmlText.StripIllegalXmlCharacters(rss.ToString()));
        }
    }
}
