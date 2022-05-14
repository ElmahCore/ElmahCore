using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ElmahCore.Assertions;
using ElmahCore.Mvc.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("ElmahCore.Mvc.Tests")]
namespace ElmahCore.Mvc
{
    internal sealed class ErrorLogMiddleware
    {
        public delegate void ErrorLoggedEventHandler(object sender, ErrorLoggedEventArgs args);

        internal static bool ShowDebugPage = false;

        private static readonly string[] SupportedContentTypes =
        {
            "application/json",
            "application/x-www-form-urlencoded",
            "application/javascript",
            "application/soap+xml",
            "application/xhtml+xml",
            "application/xml",
            "text/html",
            "text/javascript",
            "text/plain",
            "text/xml",
            "text/markdown"
        };

        private readonly Func<HttpContext, bool> _checkPermissionAction = context => true;
        private readonly string _elmahRoot = @"/elmah";
        private readonly ErrorLog _errorLog;
        private readonly List<IErrorFilter> _filters = new List<IErrorFilter>();
        private readonly ILogger _logger;
        private readonly bool _logRequestBody = true;
        private readonly RequestDelegate _next;
        private readonly IEnumerable<IErrorNotifier> _notifiers;
        private readonly Func<HttpContext, Error, Task> _onError = (context, error) => Task.CompletedTask;

        public ErrorLogMiddleware(RequestDelegate next, ErrorLog errorLog, ILoggerFactory loggerFactory,
            IOptions<ElmahOptions> elmahOptions)
        {
            ElmahExtensions.LogMiddleware = this;
            _next = next;
            _errorLog = errorLog ?? throw new ArgumentNullException(nameof(errorLog));
            var lf = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _logger = lf.CreateLogger<ErrorLogMiddleware>();

            //return here if the elmah options is not provided
            if (elmahOptions?.Value == null)
                return;
            var options = elmahOptions.Value;

            _checkPermissionAction = options.PermissionCheck;
            _onError = options.Error;

            //Notifiers
            if (options.Notifiers != null)
                _notifiers = elmahOptions.Value.Notifiers.ToList();

            //Filters
            _filters = elmahOptions.Value?.Filters.ToList();
            foreach (var errorFilter in options.Filters) Filtering += errorFilter.OnErrorModuleFiltering;

            _logRequestBody = elmahOptions.Value?.LogRequestBody == true;


            if (!string.IsNullOrEmpty(options.FiltersConfig))
                try
                {
                    ConfigureFilters(options.FiltersConfig);
                }
                catch (Exception)
                {
                    _logger.LogError("Error in filters XML file");
                }

            if (!string.IsNullOrEmpty(options.Path))
            {
                _elmahRoot = elmahOptions.Value.Path.ToLower();
                if (!_elmahRoot.StartsWith("/")) _elmahRoot = "/" + _elmahRoot;
                if (_elmahRoot.EndsWith("/")) _elmahRoot = _elmahRoot.Substring(0, _elmahRoot.Length - 1);
            }

            if (!string.IsNullOrWhiteSpace(options.ApplicationName))
                _errorLog.ApplicationName = elmahOptions.Value.ApplicationName;
            if (options.SourcePaths != null && options.SourcePaths.Any())
                _errorLog.SourcePaths = elmahOptions.Value.SourcePaths;
        }

        public event ExceptionFilterEventHandler Filtering;

        // ReSharper disable once EventNeverSubscribedTo.Global
        public event ErrorLoggedEventHandler Logged;

        private void ConfigureFilters(string config)
        {
            var doc = new XmlDocument();
            doc.Load(config);
            var filterNodes = doc.SelectNodes("//errorFilter");
            if (filterNodes != null)
                foreach (XmlNode filterNode in filterNodes)
                {
                    var notList = new List<string>();
                    var notifiers = filterNode.SelectNodes("//notifier");
                    {
                        if (notifiers != null)
                            foreach (XmlElement notifier in notifiers)
                            {
                                var name = notifier.Attributes["name"]?.Value;
                                if (name != null) notList.Add(name);
                            }
                    }
                    var assertionNode = (XmlElement) filterNode.SelectSingleNode("test/*");

                    if (assertionNode != null)
                    {
                        var a = AssertionFactory.Create(assertionNode);
                        var filter = new ErrorFilter(a, notList);
                        Filtering += filter.OnErrorModuleFiltering;
                        _filters.Add(filter);
                    }
                }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string body = null;
            try
            {
                context.Features.Set(new ElmahLogFeature());

                var sourcePath = context.Request.Path.Value;
                if (sourcePath.Equals(_elmahRoot, StringComparison.InvariantCultureIgnoreCase)
                    || sourcePath.StartsWith(_elmahRoot + "/", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!_checkPermissionAction(context))
                    {
                        await context.ChallengeAsync();
                        return;
                    }

                    var path = sourcePath.Substring(_elmahRoot.Length, sourcePath.Length - _elmahRoot.Length);
                    if (path.StartsWith("/")) path = path.Substring(1);
                    if (path.Contains('?')) path = path.Substring(0, path.IndexOf('?'));
                    await ProcessElmahRequest(context, path);
                    return;
                }

                var ct = context.Request.ContentType?.ToLower();
                var tEnc = string.Join(",", context.Request.Headers["Transfer-Encoding"].ToArray());
                if (_logRequestBody && !string.IsNullOrEmpty(ct) && SupportedContentTypes.Any(i => ct.Contains(ct))
                    && !tEnc.Contains("chunked"))
                    body = await GetBody(context.Request);

                await _next(context);


                if (context.Response.HasStarted
                    || context.Response.StatusCode < 400
                    || context.Response.StatusCode >= 600
                    || context.Response.ContentLength.HasValue
                    || !string.IsNullOrEmpty(context.Response.ContentType))
                    return;
                await LogException(new HttpException(context.Response.StatusCode), context, _onError, body);
            }
            catch (Exception exception)
            {
                var id = await LogException(exception, context, _onError, body);
                var location = $"{_elmahRoot}/detail/{id}";

                context.Features.Set<IElmahFeature>(new ElmahFeature(id, location));

                //To next middleware
                if (!ShowDebugPage) throw;
                //Show Debug page
                context.Response.Redirect(location);
            }
        }

        private async Task<string> GetBody(HttpRequest request)
        {
            request.EnableBuffering();
            var body = request.Body;
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            // ReSharper disable once MustUseReturnValue
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            body.Seek(0, SeekOrigin.Begin);
            request.Body = body;

            return bodyAsText;
        }

        private async Task ProcessElmahRequest(HttpContext context, string resource)
        {
            try
            {
                if (resource.StartsWith("api/"))
                {
                    await ErrorApiHandler.ProcessRequest(context, _errorLog, resource);
                    return;
                }

                if (resource.StartsWith("exception/"))
                {
                    await MsdnHandler.ProcessRequestException(context, resource.Substring("exception/".Length));
                    return;
                }

                if (resource.StartsWith("status/"))
                {
                    await MsdnHandler.ProcessRequestStatus(context, resource.Substring("status/".Length));
                    return;
                }

                switch (resource)
                {
                    case "xml":
                        await ErrorXmlHandler.ProcessRequest(context, _errorLog);
                        break;
                    case "json":
                        await ErrorJsonHandler.ProcessRequest(context, _errorLog);
                        break;
                    case "rss":
                        await ErrorRssHandler.ProcessRequest(context, _errorLog, _elmahRoot);
                        break;
                    case "digestrss":
                        await ErrorDigestRssHandler.ProcessRequest(context, _errorLog, _elmahRoot);
                        return;
                    case "download":
                        await ErrorLogDownloadHandler.ProcessRequestAsync(_errorLog, context);
                        return;
                    case "test":
                        throw new TestException();
                    default:
                        await ErrorResourceHandler.ProcessRequest(context, resource, _elmahRoot);
                        break;
                }
            }
            catch (TestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elmah request processing error");
            }
        }


        internal async Task<string> LogException(Exception e, HttpContext context,
            Func<HttpContext, Error, Task> onError, string body = null)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            //
            // Fire an event to check if listeners want to filter out
            // logging of the uncaught exception.
            //

            ErrorLogEntry entry = null;

            try
            {
                var args = new ExceptionFilterEventArgs(e, context);
                if (_filters.Any())
                {
                    OnFiltering(args);

                    if (args.Dismissed && !args.DismissedNotifiers.Any())
                        return null;
                }

                //
                // AddMessage away...
                //
                var error = new Error(e, context, body);

                await onError(context, error);
                var log = _errorLog;
                error.ApplicationName = log.ApplicationName;
                var id = await log.LogAsync(error);
                entry = new ErrorLogEntry(log, id, error);

                //Send notification
                foreach (var notifier in _notifiers)
                    if (!args.DismissedNotifiers.Any(i =>
                        i.Equals(notifier.Name, StringComparison.InvariantCultureIgnoreCase)))
                        notifier.Notify(error);
            }
            catch (Exception ex)
            {
                //
                // IMPORTANT! We swallow any exception raised during the 
                // logging and send them out to the trace . The idea 
                // here is that logging of exceptions by itself should not 
                // be  critical to the overall operation of the application.
                // The bad thing is that we catch ANY kind of exception, 
                // even system ones and potentially let them slip by.
                //

                _logger.LogError(ex, "Elmah local exception");
            }

            if (entry != null)
                OnLogged(new ErrorLoggedEventArgs(entry));

            return entry?.Id;
        }

        /// <summary>
        ///     Raises the <see cref="Logged" /> event.
        /// </summary>
        private void OnLogged(ErrorLoggedEventArgs args)
        {
            Logged?.Invoke(this, args);
        }

        /// <summary>
        ///     Raises the <see cref="Filtering" /> event.
        /// </summary>
        private void OnFiltering(ExceptionFilterEventArgs args)
        {
            Filtering?.Invoke(this, args);
        }
    }
}