using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;

namespace ElmahCore.Mvc.Handlers
{
    internal static class ErrorLogDownloadHandler
    {
        private const int PageSize = 100;

        private static T ProcessRequestPrelude<T>(HttpContext context, Func<Format, int, T> resultor)
        {
            Debug.Assert(context != null);
            Debug.Assert(resultor != null);

            var request = context.Request;
            var query = request.Query;

            //
            // Limit the download by some maximum # of records?
            //

            var maxDownloadCount = Math.Max(0, Convert.ToInt32(query["limit"], CultureInfo.InvariantCulture));

            //
            // Determine the desired output format.
            //

            var format = GetFormat(context, query["format"].ToString().ToLowerInvariant());
            Debug.Assert(format != null);

            //
            // Emit format header, initialize and then fetch results.
            //

            return resultor(format, maxDownloadCount);
        }

        public static Task ProcessRequestAsync(ErrorLog errorLog, HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return ProcessRequestPrelude(context,
                (format, maxDownloadCount) => ProcessRequestAsync(errorLog, context, format, maxDownloadCount));
        }

        private static async Task ProcessRequestAsync(ErrorLog log, HttpContext context, Format format, int maxDownloadCount)
        {
            await format.Header();
            
            var errorEntryList = new List<ErrorLogEntry>(PageSize);
            var downloadCount = 0;

            for (var pageIndex = 0;; pageIndex++)
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

                format.Entries(errorEntryList, 0, count, total);
            
                downloadCount += count;

                await format.FlushAsync();

                //
                // Done if either the end of the list (no more errors found) or
                // the requested limit has been reached.
                //
                if (count == 0 || downloadCount == maxDownloadCount)
                {
                    if (count > 0)
                    {
                        format.Entries(new ErrorLogEntry[0], total); // Terminator
                        await format.FlushAsync();
                    }

                    break;
                }

                //
                // Fetch next page of results.
                //
                errorEntryList.Clear();
            }
        }

        private static Format GetFormat(HttpContext context, string format)
        {
            Debug.Assert(context != null);
            var buffer = new FileBufferingWriteStream();
            switch (format)
            {
                case "jsonp": return new JsonPaddingFormat(context, buffer);
                case "html-jsonp": return new JsonPaddingFormat(context, buffer, /* wrapped */ true);
                default:
                    return new CsvFormat(context, buffer);
            }
        }

        private abstract class Format
        {
            protected Format(HttpContext context, FileBufferingWriteStream stream)
            {
                Debug.Assert(context != null);
                Context = context;
                OutputStream = stream;
            }

            protected HttpContext Context { get; }

            protected FileBufferingWriteStream OutputStream { get; }

            public abstract Task Header();

            public void Entries(IList<ErrorLogEntry> entries, int total)
            {
                Entries(entries, 0, entries.Count, total);
            }

            public abstract void Entries(IList<ErrorLogEntry> entries, int index, int count, int total);

            public Task FlushAsync()
            {
                return this.OutputStream.DrainBufferAsync(this.Context.Response.Body);
            }
        }

        private sealed class CsvFormat : Format
        {
            public CsvFormat(HttpContext context, FileBufferingWriteStream stream) :
                base(context, stream)
            {
            }

            public override async Task Header()
            {
                var response = Context.Response;
                response.Headers.Add("Content-Type", "text/csv; header=present");
                response.Headers.Add("Content-Disposition", "attachment; filename=errorlog.csv");

                await this.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(
                    "Application,Host,Time,Unix Time,Type,Source,User,Status Code,Message,URL,XMLREF,JSONREF\r\n"));
            }

            public override void Entries(IList<ErrorLogEntry> entries, int index, int count, int total)
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
                    var query = "?id=" + Uri.EscapeDataString(entry.Id);
                    var requestUrl = $"{Context.Request.Scheme}://{Context.Request.Host}{Context.Request.Path}";

                    csv.Field(error.ApplicationName)
                        .Field(error.HostName)
                        .Field(time.ToString("yyyy-MM-dd HH:mm:ss", culture))
                        .Field(time.Subtract(epoch).TotalSeconds.ToString("0.0000", culture))
                        .Field(error.Type)
                        .Field(error.Source)
                        .Field(error.User)
                        .Field(error.StatusCode.ToString(culture))
                        .Field(error.Message)
                        .Field($"{requestUrl}detail{query}")
                        .Field($"{requestUrl}detail{query}")
                        .Field($"{requestUrl}detail{query}")
                        .Record();
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

            private string _callback;

            public JsonPaddingFormat(HttpContext context, FileBufferingWriteStream stream) :
                this(context, stream, false)
            {
            }

            public JsonPaddingFormat(HttpContext context, FileBufferingWriteStream stream, bool wrapped) :
                base(context, stream)
            {
                _wrapped = wrapped;
            }

            public override async Task Header()
            {
                var callback = Context.Request.Query["callback"].FirstOrDefault()
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

                var response = Context.Response;

                if (!_wrapped)
                {
                    response.Headers.Add("Content-Type", "text/javascript");
                    response.Headers.Add("Content-Disposition", "attachment; filename=errorlog.js");
                }
                else
                {
                    response.Headers.Add("Content-Type", "text/html");

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

            public override void Entries(IList<ErrorLogEntry> entries, int index, int count, int total)
            {
                Debug.Assert(entries != null);
                Debug.Assert(index >= 0);
                Debug.Assert(index + count <= entries.Count);

                var writer = new StreamWriter(this.OutputStream) { NewLine = "\n" };

                if (_wrapped)
                {
                    writer.WriteLine("<script type='text/javascript' language='javascript'>");
                    writer.WriteLine("//<[!CDATA[");
                }

                writer.Write(_callback);
                writer.Write('(');

                var json = new Utf8JsonWriter(this.OutputStream);
                json.WriteStartObject();
                json.WriteNumber("total", total);
                json.WriteStartArray("errors");

                //var json = new JsonTextWriter(writer);
                //json.Object()
                //    .Member("total").Number(total)
                //    .Member("errors").Array();

                var requestUrl = $"{Context.Request.Scheme}://{Context.Request.Host}{Context.Request.Path}";

                for (var i = index; i < count; i++)
                {
                    var entry = entries[i];
                    //writer.WriteLine();
                    //if (i == 0) writer.Write(' ');
                    //writer.Write("  ");

                    var urlTemplate = $"{requestUrl}?id=" + Uri.EscapeDataString(entry.Id);

                    json.WriteStartObject();
                    EncodeMembers(entry.Error, json);
                    json.WriteStartArray("hrefs");

                    json.WriteStartObject();
                    json.WriteString("type", "text/html");
                    json.WriteString("href", string.Format(urlTemplate, "detail"));
                    json.WriteEndObject();

                    json.WriteStartObject();
                    json.WriteString("type", "application/json");
                    json.WriteString("href", string.Format(urlTemplate, "json"));
                    json.WriteEndObject();

                    json.WriteStartObject();
                    json.WriteString("type", "application/xml");
                    json.WriteString("href", string.Format(urlTemplate, "xml"));
                    json.WriteEndObject();

                    json.WriteEndArray();
                    json.WriteEndObject();
                }

                json.WriteEndArray();
                json.WriteEndObject();
                
                if (count > 0)
                {
                    writer.WriteLine();
                }

                writer.WriteLine(");");

                if (_wrapped)
                {
                    writer.WriteLine("//]]>");
                    writer.WriteLine("</script>");

                    if (count == 0)
                    {
                        writer.WriteLine(@"</body></html>");
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
                Member(writer, "webHostHtmlMessage", error.WebHostHtmlMessage);
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
                var items = from i in Enumerable.Range(0, collection.Count)
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
                            };

                var list = items.ToList();
                if (!list.Any())
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
            private static readonly char[] Reserved = {'\"', ',', '\r', '\n'};
            private readonly TextWriter _writer;
            private int _column;

            public CsvWriter(TextWriter writer)
            {
                Debug.Assert(writer != null);

                _writer = writer;
            }

            // ReSharper disable once UnusedMethodReturnValue.Local
            public CsvWriter Record()
            {
                _writer.WriteLine();
                _column = 0;
                return this;
            }

            public CsvWriter Field(string value)
            {
                if (_column > 0)
                {
                    _writer.Write(',');
                }

                // 
                // Fields containing line breaks (CRLF), double quotes, and commas 
                // need to be enclosed in double-quotes. 
                //
                var index = value.IndexOfAny(Reserved);

                if (index < 0)
                {
                    _writer.Write(value);
                }
                else
                {
                    //
                    // As double-quotes are used to enclose fields, then a 
                    // double-quote appearing inside a field must be escaped by 
                    // preceding it with another double quote. 
                    //

                    const string quote = "\"";
                    _writer.Write(quote);
                    _writer.Write(value.Replace(quote, quote + quote));
                    _writer.Write(quote);
                }

                _column++;
                return this;
            }
        }
    }
}