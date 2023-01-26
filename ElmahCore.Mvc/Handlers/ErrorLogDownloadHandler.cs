using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        private static async Task ProcessRequestAsync(ErrorLog log, HttpContext context, Format format,
            int maxDownloadCount)
        {
            var response = context.Response;
            var output = response;

            foreach (var text in format.Header())
                await output.WriteAsync(text);

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
                        count += remaining;
                }

                foreach (var entry in format.Entries(errorEntryList, 0, count, total))
                    await response.WriteAsync(entry);

                downloadCount += count;

                await response.Body.FlushAsync();

                //
                // Done if either the end of the list (no more errors found) or
                // the requested limit has been reached.
                //

                if (count == 0 || downloadCount == maxDownloadCount)
                {
                    if (count > 0)
                        foreach (var entry in format.Entries(new ErrorLogEntry[0], total)) // Terminator
                            await response.WriteAsync(entry);
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
            switch (format)
            {
                case "jsonp": return new JsonPaddingFormat(context);
                case "html-jsonp": return new JsonPaddingFormat(context, /* wrapped */ true);
                default:
                    return new CsvFormat(context);
            }
        }

        private abstract class Format
        {
            protected Format(HttpContext context)
            {
                Debug.Assert(context != null);
                Context = context;
            }

            protected HttpContext Context { get; }

            public virtual IEnumerable<string> Header()
            {
                yield break;
            }

            public IEnumerable<string> Entries(IList<ErrorLogEntry> entries, int total)
            {
                return Entries(entries, 0, entries.Count, total);
            }

            public abstract IEnumerable<string> Entries(IList<ErrorLogEntry> entries, int index, int count, int total);
        }

        private sealed class CsvFormat : Format
        {
            public CsvFormat(HttpContext context) :
                base(context)
            {
            }

            public override IEnumerable<string> Header()
            {
                var response = Context.Response;
                response.Headers.Add("Content-Type", "text/csv; header=present");
                response.Headers.Add("Content-Disposition", "attachment; filename=errorlog.csv");
                yield return
                    "Application,Host,Time,Unix Time,Type,Source,User,Status Code,Message,URL,XMLREF,JSONREF\r\n";
            }

            public override IEnumerable<string> Entries(IList<ErrorLogEntry> entries, int index, int count, int total)
            {
                Debug.Assert(entries != null);
                Debug.Assert(index >= 0);
                Debug.Assert(index + count <= entries.Count);

                if (count == 0)
                    yield break;

                //
                // Setup to emit CSV records.
                //

                var writer = new StringWriter {NewLine = "\r\n"};
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

                yield return writer.ToString();
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

            public JsonPaddingFormat(HttpContext context) :
                this(context, false)
            {
            }

            public JsonPaddingFormat(HttpContext context, bool wrapped) :
                base(context)
            {
                _wrapped = wrapped;
            }

            public override IEnumerable<string> Header()
            {
                var callback = Context.Request.Query["callback"].FirstOrDefault()
                               ?? string.Empty;

                if (callback.Length == 0)
                    throw new Exception("The JSONP callback parameter is missing.");

                if (!CallbackExpression.IsMatch(callback))
                    throw new Exception("The JSONP callback parameter is not in an acceptable format.");

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

                    yield return
                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";
                    yield return @"
                    <html xmlns='http://www.w3.org/1999/xhtml'>
                    <head>
                        <title>Error AddMessage in HTML-Wrapped JSONP Format</title>
                    </head>
                    <body>
                        <p>This page is primarily designed to be used in an IFRAME of a parent HTML document.</p>";
                }
            }

            public override IEnumerable<string> Entries(IList<ErrorLogEntry> entries, int index, int count, int total)
            {
                Debug.Assert(entries != null);
                Debug.Assert(index >= 0);
                Debug.Assert(index + count <= entries.Count);

                var writer = new StringWriter {NewLine = "\n"};

                if (_wrapped)
                {
                    writer.WriteLine("<script type='text/javascript' language='javascript'>");
                    writer.WriteLine("//<[!CDATA[");
                }

                writer.Write(_callback);
                writer.Write('(');

                var json = new JsonTextWriter(writer);
                json.Object()
                    .Member("total").Number(total)
                    .Member("errors").Array();

                var requestUrl = $"{Context.Request.Scheme}://{Context.Request.Host}{Context.Request.Path}";

                for (var i = index; i < count; i++)
                {
                    var entry = entries[i];
                    writer.WriteLine();
                    if (i == 0) writer.Write(' ');
                    writer.Write("  ");

                    var urlTemplate = $"{requestUrl}?id=" + Uri.EscapeDataString(entry.Id);

                    json.Object();
                    ErrorJson.EncodeMembers(entry.Error, json);
                    json.Member("hrefs")
                        .Array()
                        .Object()
                        .Member("type").String("text/html")
                        .Member("href").String(string.Format(urlTemplate, "detail")).Pop()
                        .Object()
                        .Member("type").String("application/json")
                        .Member("href").String(string.Format(urlTemplate, "json")).Pop()
                        .Object()
                        .Member("type").String("application/xml")
                        .Member("href").String(string.Format(urlTemplate, "xml")).Pop()
                        .Pop()
                        .Pop();
                }

                json.Pop();
                json.Pop();

                if (count > 0)
                    writer.WriteLine();

                writer.WriteLine(");");

                if (_wrapped)
                {
                    writer.WriteLine("//]]>");
                    writer.WriteLine("</script>");

                    if (count == 0)
                        writer.WriteLine(@"</body></html>");
                }

                yield return writer.ToString();
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
                    _writer.Write(',');

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