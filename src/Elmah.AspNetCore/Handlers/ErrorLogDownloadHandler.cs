using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Elmah.AspNetCore.Handlers;

internal static partial class Endpoints
{
    private const int PageSize = 100;

    public static IEndpointConventionBuilder MapDownload(this IEndpointRouteBuilder builder, string prefix = "")
    {
        var handler = RequestDelegateFactory.Create(async (
            [FromQuery] string? format,
            [FromQuery] int? limit,
            [FromServices] ErrorLog errorLog,
            HttpContext context) =>
        {
            var buffer = new FileBufferingWriteStream();

            //
            // Determine the desired output format.
            //
            var downloadFormat = GetFormat(format?.ToLowerInvariant() ?? "csv", buffer);

            await ProcessRequestAsync(errorLog, context, downloadFormat, limit ?? 0);

            await buffer.DrainBufferAsync(context.Response.Body);

            return Results.Ok();
        });

        var pipeline = builder.CreateApplicationBuilder();
        pipeline.Run(handler.RequestDelegate);
        return builder.MapMethods($"{prefix}/download", new[] { HttpMethods.Get, HttpMethods.Post }, pipeline.Build());
    }

    private static async Task ProcessRequestAsync(ErrorLog log, HttpContext context, Format format, int maxDownloadCount)
    {
        await format.WriteHeaderAsync(context.Response);

        var errorEntryList = new List<ErrorLogEntry>(PageSize);
        var downloadCount = 0;

        for (var pageIndex = 0; ; pageIndex++)
        {
            var total = await log.GetErrorsAsync(null, new List<ErrorLogFilter>(), pageIndex, PageSize, errorEntryList);
            var count = errorEntryList.Count;

            if (maxDownloadCount > 0)
            {
                var remaining = maxDownloadCount - (downloadCount + count);
                if (remaining < 0)
                {
                    count += remaining;
                }
            }

            await format.WriteEntriesAsync(context, errorEntryList, 0, count, total);

            downloadCount += count;

            //
            // Done if either the end of the list (no more errors found) or
            // the requested limit has been reached.
            //
            if (count == 0 || downloadCount == maxDownloadCount)
            {
                if (count > 0)
                {
                    await format.WriteEntriesAsync(context, new ErrorLogEntry[0], total); // Terminator
                }

                break;
            }

            //
            // Fetch next page of results.
            //
            errorEntryList.Clear();
        }
    }

    private static Format GetFormat(string format, Stream stream)
    {
        switch (format)
        {
            case "jsonp": return new JsonPaddingFormat(stream);
            case "html-jsonp": return new JsonPaddingFormat(stream, wrapped: true);
            default:
                return new CsvFormat(stream);
        }
    }

    private abstract class Format
    {
        protected Format(Stream stream)
        {
            OutputStream = stream;
        }

        protected Stream OutputStream { get; }

        public abstract Task WriteHeaderAsync(HttpResponse response);

        public Task WriteEntriesAsync(HttpContext context, IList<ErrorLogEntry> entries, int total)
        {
            return WriteEntriesAsync(context, entries, 0, entries.Count, total);
        }

        public abstract Task WriteEntriesAsync(HttpContext context, IList<ErrorLogEntry> entries, int index, int count, int total);
    }

    private sealed class CsvFormat : Format
    {
        public CsvFormat(Stream stream) : base(stream)
        {
        }

        public override async Task WriteHeaderAsync(HttpResponse response)
        {
            response.Headers[HeaderNames.ContentType] = "text/csv; header=present";
            response.Headers[HeaderNames.ContentDisposition] = "attachment; filename=errorlog.csv";

            await this.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(
                "Application,Host,Time,Unix Time,Type,Source,User,Status Code,Message,URL,XMLREF,JSONREF\r\n"));
        }

        public override async Task WriteEntriesAsync(HttpContext context, IList<ErrorLogEntry> entries, int index, int count, int total)
        {
            Debug.Assert(entries != null);
            Debug.Assert(index >= 0);
            Debug.Assert(index + count <= entries.Count);

            if (count == 0)
            {
                return;
            }

            //
            // Setup to emit CSV records.
            //
            var writer = new StreamWriter(this.OutputStream) { NewLine = "\r\n" };
            var csv = new CsvWriter(writer);

            var culture = CultureInfo.InvariantCulture;
            var epoch = new DateTime(1970, 1, 1);

            //
            // For each error, emit a CSV record.
            //
            for (var i = index; i < count; i++)
            {
                var entry = entries[i];
                var error = entry.Error;
                var time = error.Time.ToUniversalTime();
                var query = "?id=" + Uri.EscapeDataString(entry.Id.ToString());
                var requestUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";

                await csv.Field(error.ApplicationName);
                await csv.Field(error.HostName);
                await csv.Field(time.ToString("yyyy-MM-dd HH:mm:ss", culture));
                await csv.Field(time.Subtract(epoch).TotalSeconds.ToString("0.0000", culture));
                await csv.Field(error.Type);
                await csv.Field(error.Source);
                await csv.Field(error.User);
                await csv.Field(error.StatusCode.ToString(culture));
                await csv.Field(error.Message);
                await csv.Field($"{requestUrl}detail{query}");
                await csv.Field($"{requestUrl}detail{query}");
                await csv.Field($"{requestUrl}detail{query}");
                await csv.Record();
            }
        }
    }

    private sealed class JsonPaddingFormat : Format
    {
        private static readonly Regex CallbackExpression = new Regex(@"^ 
                     [a-z_] [a-z0-9_]+ ( \[ [0-9]+ \] )?
                ( \. [a-z_] [a-z0-9_]+ ( \[ [0-9]+ \] )? )* $",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.ExplicitCapture
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.CultureInvariant);

        private readonly bool _wrapped;
        private string _callback = string.Empty;

        public JsonPaddingFormat(Stream stream) :
            this(stream, false)
        {
        }

        public JsonPaddingFormat(Stream stream, bool wrapped) :
            base(stream)
        {
            _wrapped = wrapped;
        }

        public override async Task WriteHeaderAsync(HttpResponse response)
        {
            var callback = response.HttpContext.Request.Query["callback"].FirstOrDefault()
                           ?? string.Empty;

            if (callback.Length == 0)
            {
                throw new Exception("The JSONP callback parameter is missing.");
            }

            if (!CallbackExpression.IsMatch(callback))
            {
                throw new Exception("The JSONP callback parameter is not in an acceptable format.");
            }

            _callback = callback;

            if (!_wrapped)
            {
                response.Headers[HeaderNames.ContentType] = "text/javascript";
                response.Headers[HeaderNames.ContentDisposition] = "attachment; filename=errorlog.js";
            }
            else
            {
                response.Headers[HeaderNames.ContentType] = MediaTypeNames.Text.Html;

                await this.OutputStream.WriteAsync(
                    Encoding.UTF8.GetBytes(
                    "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">"));

                await this.OutputStream.WriteAsync(
                    Encoding.UTF8.GetBytes(@"
                    <html xmlns='http://www.w3.org/1999/xhtml'>
                    <head>
                        <title>Error AddMessage in HTML-Wrapped JSONP Format</title>
                    </head>
                    <body>
                        <p>This page is primarily designed to be used in an IFRAME of a parent HTML document.</p>"));
            }
        }

        public override async Task WriteEntriesAsync(HttpContext context, IList<ErrorLogEntry> entries, int index, int count, int total)
        {
            Debug.Assert(entries != null);
            Debug.Assert(index >= 0);
            Debug.Assert(index + count <= entries.Count);

            var writer = new StreamWriter(this.OutputStream) { NewLine = "\n" };

            if (_wrapped)
            {
                await writer.WriteLineAsync("<script type='text/javascript' language='javascript'>");
                await writer.WriteLineAsync("//<[!CDATA[");
            }

            await writer.WriteAsync(_callback);
            await writer.WriteAsync('(');

            var json = new Utf8JsonWriter(this.OutputStream);
            json.WriteStartObject();
            json.WriteNumber("total", total);
            json.WriteStartArray("errors");

            var requestUrl = context.GetElmahAbsoluteRoot();

            for (var i = index; i < count; i++)
            {
                var entry = entries[i];
                var urlTemplate = $"{requestUrl}?id=" + Uri.EscapeDataString(entry.Id.ToString());

                json.WriteStartObject();
                EncodeMembers(entry.Error, json);
                json.WriteStartArray("hrefs");

                json.WriteStartObject();
                json.WriteString("type", MediaTypeNames.Text.Html);
                json.WriteString("href", string.Format(urlTemplate, "detail"));
                json.WriteEndObject();

                json.WriteStartObject();
                json.WriteString("type", MediaTypeNames.Application.Json);
                json.WriteString("href", string.Format(urlTemplate, "json"));
                json.WriteEndObject();

                json.WriteStartObject();
                json.WriteString("type", MediaTypeNames.Application.Xml);
                json.WriteString("href", string.Format(urlTemplate, "xml"));
                json.WriteEndObject();

                json.WriteEndArray();
                json.WriteEndObject();
            }

            json.WriteEndArray();
            json.WriteEndObject();
            await json.FlushAsync();

            if (count > 0)
            {
                await writer.WriteLineAsync();
            }

            await writer.WriteLineAsync(");");

            if (_wrapped)
            {
                await writer.WriteLineAsync("//]]>");
                await writer.WriteLineAsync("</script>");

                if (count == 0)
                {
                    await writer.WriteLineAsync(@"</body></html>");
                }
            }
        }

        private static void EncodeMembers(Error error, Utf8JsonWriter writer)
        {
            Member(writer, "application", error.ApplicationName);
            Member(writer, "host", error.HostName);
            Member(writer, "type", error.Type);
            Member(writer, "message", error.Message);
            Member(writer, "source", error.Source);
            Member(writer, "detail", error.Detail);
            Member(writer, "user", error.User);
            Member(writer, "time", error.Time, DateTime.MinValue);
            Member(writer, "statusCode", error.StatusCode, 0);
            Member(writer, "serverVariables", error.ServerVariables);
            Member(writer, "queryString", error.QueryString);
            Member(writer, "form", error.Form);
            Member(writer, "cookies", error.Cookies);
        }

        private static void Member(Utf8JsonWriter writer, string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            writer.WriteString(name, value);
        }

        private static void Member(Utf8JsonWriter writer, string name, DateTime value, DateTime defaultValue)
        {
            if (value == defaultValue)
            {
                return;
            }

            writer.WriteString(name, XmlConvert.ToString(value, XmlDateTimeSerializationMode.Utc));
        }

        private static void Member(Utf8JsonWriter writer, string name, int value, int defaultValue)
        {
            if (value == defaultValue)
            {
                return;
            }

            writer.WriteNumber(name, value);
        }

        private static void Member(Utf8JsonWriter writer, string name, NameValueCollection collection)
        {
            Debug.Assert(writer != null);

            //
            // Bail out early if the collection is null or empty.
            //
            if (collection == null || collection.Count == 0)
            {
                return;
            }

            //
            // For each key, we get all associated values and loop through
            // twice. The first time round, we count strings that are 
            // neither null nor empty. If none are found then the key is 
            // skipped. Otherwise, second time round, we encode
            // strings that are neither null nor empty. If only such string
            // exists for a key then it is written directly, otherwise
            // multiple strings are naturally wrapped in an array.
            //
            var items = (from i in Enumerable.Range(0, collection.Count)
                        let values = collection.GetValues(i)
                        where values != null && values.Length > 0
                        let some = // Neither null nor empty
                            from v in values
                            where !string.IsNullOrEmpty(v)
                            select v
                        let nom = some.Take(2).Count()
                        where nom > 0
                        select new
                        {
                            Key = collection.GetKey(i),
                            IsArray = nom > 1,
                            Values = some
                        }).ToList();

            if (!items.Any())
            {
                return;
            }

            //
            // There is at least one value so now we emit the key.
            // Before doing that, we check if the collection member
            // was ever started. If not, this would be a good time.
            //
            writer.WriteStartObject(name);

            foreach (var item in items)
            {
                writer.WritePropertyName(item.Key ?? string.Empty);

                if (item.IsArray)
                {
                    writer.WriteStartArray(); // Wrap multiples in an array
                }

                foreach (var value in item.Values)
                {
                    writer.WriteStringValue(value);
                }

                if (item.IsArray)
                {
                    writer.WriteEndArray(); // Close multiples array
                }
            }

            writer.WriteEndObject();
        }
    }

    private sealed class CsvWriter
    {
        private static readonly char[] Reserved = { '\"', ',', '\r', '\n' };
        private readonly TextWriter _writer;
        private int _column;

        public CsvWriter(TextWriter writer)
        {
            Debug.Assert(writer != null);

            _writer = writer;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        public Task Record()
        {
            _column = 0;
            return _writer.WriteLineAsync();
        }

        public async Task Field(string value)
        {
            if (_column > 0)
            {
                await _writer.WriteAsync(',');
            }

            // 
            // Fields containing line breaks (CRLF), double quotes, and commas 
            // need to be enclosed in double-quotes. 
            //
            var index = value.IndexOfAny(Reserved);

            if (index < 0)
            {
                await _writer.WriteAsync(value);
            }
            else
            {
                //
                // As double-quotes are used to enclose fields, then a 
                // double-quote appearing inside a field must be escaped by 
                // preceding it with another double quote. 
                //
                const string quote = "\"";
                await _writer.WriteAsync(quote);
                await _writer.WriteAsync(value.Replace(quote, quote + quote));
                await _writer.WriteAsync(quote);
            }

            _column++;
        }
    }
}