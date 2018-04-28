using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ElmahCore.Assertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ElmahCore
{
    internal class ErrorLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ErrorLog _errorLog;
        private readonly IEnumerable<IErrorNotifier> _notifiers;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly DiagnosticSource _diagnosticSource;
        public event ExceptionFilterEventHandler Filtering;
        public event ErrorLoggedEventHandler Logged;
        private readonly string _elmahRoot = @"/elmah";
        private readonly List<IErrorFilter> _filters = new List<IErrorFilter>();
        private ILogger _logger;

        public delegate void ErrorLoggedEventHandler(object sender, ErrorLoggedEventArgs args);

        public ErrorLogMiddleware(RequestDelegate next, ErrorLog errorLog,
            ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IOptions<ElmahOptions> elmahOptions)
        {
            _errorLog = errorLog ?? throw new ArgumentNullException(nameof(errorLog));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));

            _logger = _loggerFactory.CreateLogger<ErrorLogMiddleware>();

            //Notifiers
            if (elmahOptions.Value.Notifiers != null)
                _notifiers = elmahOptions.Value.Notifiers.ToList();

            //Filters
            if (elmahOptions.Value.Filters != null)
            {
                _filters = elmahOptions.Value.Filters.ToList();
                foreach (var errorFilter in _filters)
                {
                    Filtering += errorFilter.OnErrorModuleFiltering;
                }
            }


            if (!string.IsNullOrEmpty(elmahOptions.Value.FiltersConfig))
            {
                try
                {
                    ConfigureFilters(elmahOptions.Value.FiltersConfig);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error in filters XML file", e);
                }
            }

            if (!string.IsNullOrEmpty(elmahOptions.Value?.Path))
            {
                _elmahRoot = elmahOptions.Value.Path.ToLower();
            }
            if (!_elmahRoot.StartsWith("/")) _elmahRoot = "/" + _elmahRoot;
            if (_elmahRoot.EndsWith("/")) _elmahRoot = _elmahRoot.Substring(0,_elmahRoot.Length-1);

            _next = next;
        }

        private void ConfigureFilters(string config)
        {
            var doc = new XmlDocument();
            doc.Load(config);
            var filterNodes = doc.SelectNodes("//errorFilter");
            foreach (XmlNode filterNode in filterNodes)
            {
                var notList = new List<string>();
                var notifiers = filterNode.SelectNodes("//notifier");
                {
                    foreach (XmlElement notifier in notifiers)
                    {
                        var name = notifier.Attributes["name"]?.Value;
                        if (name != null)
                        {
                            notList.Add(name);
                        }
                    }
                }
                var assertionNode = (XmlElement)filterNode.SelectSingleNode("test/*");

                if (assertionNode != null)
                {
                    var a = AssertionFactory.Create(assertionNode);
                    var filter = new ErrorFilter(a, notList);
                    Filtering += filter.OnErrorModuleFiltering;
                    _filters.Add(filter);
                }
            }

        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (context.Request.Path.Value.Equals(_elmahRoot,StringComparison.InvariantCultureIgnoreCase) || context.Request.Path.Value.StartsWith(_elmahRoot+"/", StringComparison.InvariantCultureIgnoreCase))
                {
                    await ProcessElamhRequest(context);
                    return;
                }

                await _next.Invoke(context);

                if (context.Response.HasStarted
                    || context.Response.StatusCode < 400
                    || context.Response.StatusCode >= 600
                    || context.Response.ContentLength.HasValue
                    || !string.IsNullOrEmpty(context.Response.ContentType))
                {
                   return;
                }
                LogException(new HttpException(context.Response.StatusCode), context);
            }
            catch (Exception exception)
            {
                LogException(exception, context);

                //To next middleware
                throw;
            }
        }

        private async Task ProcessElamhRequest(HttpContext context)
        {
            try
            {
                var resource = context.Request.Path.Value.Split('/').LastOrDefault();
                switch (resource)
                {
                    case "detail":
                        await ProcessTemplate<ErrorDetailPage>(context, _errorLog);
                        break;
                    case "html":
                        break;
                    case "xml":
                        ErrorXmlHandler.ProcessRequest(context, _errorLog);
                        break;
                    case "json":
                        ErrorJsonHandler.ProcessRequest(context, _errorLog);
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
                    case "stylesheet":
                        StyleSheetHelper.LoadStyleSheets(context, StyleSheetHelper.StyleSheetResourceNames, "text/css",
                            Encoding.UTF8,
                            true);
                        break;
                    case "test":
                        throw new TestException();
                    case "about":
                        await ProcessTemplate<AboutPage>(context, _errorLog);
                        break;
                    default:
                        await ProcessTemplate<ErrorLogPage>(context, _errorLog);
                        break;
                }
            }
            catch (TestException)
            {
                throw;
            }
            catch(Exception e)
            {
                _logger.LogError("Elmah request processing error", e);
            }
        }

        
        async Task ProcessTemplate<T>(HttpContext context, ErrorLog error) where T : WebTemplateBase, new()
        {
            var template = new T { Context = context, ErrorLog = error, ElmahRoot = _elmahRoot};

            await context.Response.WriteAsync(template.TransformText());
        }


        protected void LogException(Exception e, HttpContext context)
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
                        return;
                }

            //
            // Log away...
            //

                var error = new Error(e, context);
                var log = GetErrorLog(context);
                error.ApplicationName = log.ApplicationName;
                var id = log.Log(error);
                entry = new ErrorLogEntry(log, id, error);

                //Send notification
                foreach (var notifier in _notifiers)
                {
                    if (!args.DismissedNotifiers.Any(i=>i.Equals(notifier.Name,StringComparison.InvariantCultureIgnoreCase)))
                     notifier.Notify(error);
                }

            }
            catch (Exception localException)
            {
                //
                // IMPORTANT! We swallow any exception raised during the 
                // logging and send them out to the trace . The idea 
                // here is that logging of exceptions by itself should not 
                // be  critical to the overall operation of the application.
                // The bad thing is that we catch ANY kind of exception, 
                // even system ones and potentially let them slip by.
                //

                _logger.LogError("Elmah local exception", localException);
            }
            if (entry != null)
                OnLogged(new ErrorLoggedEventArgs(entry));
        }

        /// <summary>
        /// Gets the <see cref="ErrorLog"/> instance to which the module
        /// will log exceptions.
        /// </summary>

        protected virtual ErrorLog GetErrorLog(HttpContext context)
        {
            return _errorLog;
        }

        /// <summary>
        /// Raises the <see cref="Logged"/> event.
        /// </summary>

        protected virtual void OnLogged(ErrorLoggedEventArgs args)
        {
            Logged?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Filtering"/> event.
        /// </summary>

        protected virtual void OnFiltering(ExceptionFilterEventArgs args)
        {
            Filtering?.Invoke(this, args);
        }
    }
}
