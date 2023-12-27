using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace ElmahCore;

internal sealed class ErrorFactory : IErrorFactory
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        MaxDepth = 0
    };

    private static readonly string[] SupportedContentTypes =
{
        MediaTypeNames.Application.Json,
        "application/x-www-form-urlencoded",
        "application/javascript",
        MediaTypeNames.Application.Soap,
        "application/xhtml+xml",
        MediaTypeNames.Application.Xml,
        MediaTypeNames.Text.Html,
        "text/javascript",
        MediaTypeNames.Text.Plain,
        MediaTypeNames.Text.Xml,
        "text/markdown"
    };

    private readonly IOptions<ElmahOptions> _elmahOptions;

    public ErrorFactory(IOptions<ElmahOptions> elmahOptions)
    {
        _elmahOptions = elmahOptions;
    }

    public async Task<Error> CreateAsync(Exception? e, HttpContext? context)
    {
        var options = _elmahOptions.Value;
        var baseException = e?.GetBaseException();
        string? typeName = baseException?.GetType().FullName;
        int statusCode = 0;

        //
        // If this is an HTTP exception, then get the status code
        // and detailed HTML message provided by the host.
        //
        if (baseException is HttpRequestException { StatusCode: not null } httpExc)
        {
            statusCode = (int)httpExc.StatusCode;
            baseException = baseException.InnerException;
            if (baseException is null)
            {
                typeName = "HTTP";
            }
        }
        else
        {
            if (context?.Connection?.LocalIpAddress is not null)
            {
                statusCode = 500;
            }
        }

        var error = new Error
        {
            Time = DateTime.Now,
            Exception = baseException,
            Message = baseException?.Message,
            Source = baseException?.Source,
            Detail = e?.ToString(),
            StatusCode = statusCode,
            Type = typeName
        };

        //
        // Load the basic information.
        //
        try
        {
            error.HostName = Environment.MachineName;
        }
        catch (SecurityException)
        {
            // A SecurityException may occur in certain, possibly 
            // user-modified, Medium trust environments.
        }

        string? user = context?.User?.Identity?.Name;
        if (context is not null)
        {
            var webUser = context.User;
            if (webUser is not null && !string.IsNullOrEmpty(webUser.Identity?.Name))
            {
                user = webUser.Identity.Name;
            }

            var request = context.Request;

            //Load Server Variables
            var serverVariables = GetServerVariables(context);
            serverVariables.Add("HttpStatusCode", statusCode.ToString());
            error.ServerVariables = serverVariables;

            error.QueryString = CopyCollection(request.Query);
            var form = CopyCollection(request.HasFormContentType && options.LogRequestForm ? request.Form : null);

            if (options.LogRequestBody)
            {
                var body = await ReadBodyAsync(context);
                if (!string.IsNullOrEmpty(body))
                {
                    form ??= new NameValueCollection();
                    form.Add("$request-body", body);
                }
            }

            error.Form = form;
            error.Cookies = CopyCollection(options.LogRequestCookies ? request.Cookies : null);

            var feature = context.Features.Get<IElmahLogFeature>();
            if (feature is not null)
            {
                error.MessageLog = feature.Log.ToArray();
                error.SqlLog = feature.LogSql.ToArray();
                error.Params = feature.Params
                    .Where(x => x.Params.Any())
                    .Select(x => new ElmahLogParamEntry(
                        x.TimeStamp,
                        GetStringParams(x.Params),
                        x.TypeName,
                        x.MemberName,
                        x.File,
                        x.Line))
                    .ToArray();
            }
        }

        error.User = user;

        var callerInfo = e?.TryGetCallerInfo() ?? CallerInfo.Empty;
        if (!callerInfo.IsEmpty)
        {
            error.Detail = "# caller: " + callerInfo
                                   + Environment.NewLine
                                   + error.Detail;
        }

        return error;
    }

    private static KeyValuePair<string, string>[] GetStringParams((string name, object value)[] paramParams) =>
        (from param in paramParams
         where param != default
         select new KeyValuePair<string, string>(param.name, ToJsonString(param.value))).ToArray();

    private static string ToJsonString(object? paramValue)
    {
        if (paramValue is null)
        {
            return "null";
        }

        try
        {
            return JsonSerializer.Serialize(paramValue, SerializerOptions);
        }
        catch
        {
            return paramValue.ToString()!;
        }
    }

    private static NameValueCollection GetServerVariables(HttpContext context)
    {
        var serverVariables = new NameValueCollection();
        LoadVariables(serverVariables, () => context.Features, "");
        LoadVariables(serverVariables, () => context.User, "User_");

        var ss = context.RequestServices?.GetService(typeof(ISession));
        if (ss is not null)
        {
            LoadVariables(serverVariables, () => context.Session, "Session_");
        }

        LoadVariables(serverVariables, () => context.Items, "Items_");
        LoadVariables(serverVariables, () => context.Connection, "Connection_");
        return serverVariables;
    }

    private static void LoadVariables(NameValueCollection serverVariables, Func<object?> getObject, string prefix)
    {
        object? obj;
        try
        {
            obj = getObject();
            if (obj is null)
            {
                return;
            }
        }
        catch
        {
            return;
        }

        var props = obj.GetType().GetProperties();
        foreach (var prop in props)
        {
            object? value = null;
            try
            {
                value = prop.GetValue(obj);
            }
            catch
            {
                // ignored
            }

            var isProcessed = false;
            if (value is IEnumerable en && en is not string)
            {
                if (value is IDictionary<object, object> { Keys.Count: 0 })
                {
                    continue;
                }

                foreach (var item in en)
                {
                    try
                    {
                        var keyProp = item.GetType().GetProperty("Key");
                        var valueProp = item.GetType().GetProperty("Value");

                        if (keyProp is not null && valueProp is not null)
                        {
                            isProcessed = true;
                            var val = valueProp.GetValue(item);
                            if (val is not null && val.GetType().ToString() != val.ToString() &&
                                !val.GetType().IsSubclassOf(typeof(Stream)))
                            {
                                var propName =
                                    prop.Name.StartsWith("RequestHeaders",
                                        StringComparison.InvariantCultureIgnoreCase)
                                        ? "Header_"
                                        : prop.Name + "_";
                                serverVariables.Add(prefix + propName + keyProp.GetValue(item), val.ToString());
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            if (isProcessed)
            {
                continue;
            }

            try
            {
                if (value is not null && value.GetType().ToString() != value.ToString() &&
                    !value.GetType().IsSubclassOf(typeof(Stream)))
                {
                    serverVariables.Add(prefix + prop.Name, value?.ToString());
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    private static NameValueCollection? CopyCollection(IEnumerable<KeyValuePair<string, StringValues>>? collection)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        if (collection is null || !collection.Any())
        {
            return null;
        }

        // ReSharper disable once PossibleMultipleEnumeration
        var keyValuePairs = collection as KeyValuePair<string, StringValues>[] ?? collection.ToArray();
        if (!keyValuePairs.Any())
        {
            return null;
        }

        var col = new NameValueCollection();
        foreach (var pair in keyValuePairs)
        {
            col.Add(pair.Key, pair.Value);
        }

        return col;
    }

    private static NameValueCollection? CopyCollection(IRequestCookieCollection? cookies)
    {
        if (cookies is null || cookies.Count == 0)
        {
            return null;
        }

        var copy = new NameValueCollection(cookies.Count);

        foreach (var cookie in cookies)
        {
            //
            // NOTE: We drop the Path and Domain properties of the 
            // cookie for sake of simplicity.
            //
            copy.Add(cookie.Key, cookie.Value);
        }

        return copy;
    }

    public static async Task<string?> ReadBodyAsync(HttpContext context)
    {
        var ct = context.Request.ContentType?.ToLower();
        var tEnc = string.Join(",", context.Request.Headers[HeaderNames.TransferEncoding].ToArray());
        if (string.IsNullOrEmpty(ct) || tEnc.Contains("chunked") || !SupportedContentTypes.Any(i => ct.Contains(ct)))
        {
            return null;
        }

        context.Request.EnableBuffering();
        var body = context.Request.Body;
        var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];

        // ReSharper disable once MustUseReturnValue
        await context.Request.Body.ReadAsync(buffer);
        var bodyAsText = Encoding.UTF8.GetString(buffer);
        body.Seek(0, SeekOrigin.Begin);
        context.Request.Body = body;

        return bodyAsText;
    }
}
