using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ElmahCore.Assertions;
using ElmahCore.Mvc.About;
using ElmahCore.Mvc.ErrorDetail;
using ElmahCore.Mvc.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace ElmahCore.Mvc
{
    internal class ErrorLogMiddleware
    {
        private readonly ErrorLog _errorLog;
        private readonly IEnumerable<IErrorNotifier> _notifiers;
        public event ExceptionFilterEventHandler Filtering;
        public event ErrorLoggedEventHandler Logged;
        private readonly string _elmahRoot = @"/elmah";
        private readonly List<IErrorFilter> _filters = new List<IErrorFilter>();
        private readonly ILogger _logger;
        private readonly Func<HttpContext, bool> _сheckPermissionAction = context => true;
        private readonly Func<HttpContext, Error, Task> _onError = (context, error) => Task.CompletedTask;

        public delegate void ErrorLoggedEventHandler(object sender, ErrorLoggedEventArgs args);

        public ErrorLogMiddleware(ErrorLog errorLog, ILoggerFactory loggerFactory, IOptions<ElmahOptions> elmahOptions)
        {
            ElmahExtensions.LogMiddleware = this;
            _errorLog = errorLog ?? throw new ArgumentNullException(nameof(errorLog));
            var lf = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _logger = lf.CreateLogger<ErrorLogMiddleware>();

            //return here if the elmah options is not provided
            if (elmahOptions?.Value == null)
                return;
            var options = elmahOptions.Value;

	        _сheckPermissionAction = options.PermissionCheck;
            _onError = options.Error;

            //Notifiers
            if (options.Notifiers != null)
                _notifiers = elmahOptions.Value.Notifiers.ToList();

            //Filters
            foreach (var errorFilter in options.Filters)
            {
                Filtering += errorFilter.OnErrorModuleFiltering;
            }
            


            if (!string.IsNullOrEmpty(options.FiltersConfig))
            {
                try
                {
                    ConfigureFilters(options.FiltersConfig);
                }
                catch (Exception)
                {
                    _logger.LogError("Error in filters XML file");
                }
            }

            if (!string.IsNullOrEmpty(options.Path))
            {
                _elmahRoot = elmahOptions.Value.Path.ToLower();
                if (!_elmahRoot.StartsWith("/")) _elmahRoot = "/" + _elmahRoot;
                if (_elmahRoot.EndsWith("/")) _elmahRoot = _elmahRoot.Substring(0, _elmahRoot.Length - 1);
            }
          
            if (!string.IsNullOrWhiteSpace(options.ApplicationName))
            {
                _errorLog.ApplicationName = elmahOptions.Value.ApplicationName;
            }
        }

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
                                if (name != null)
                                {
                                    notList.Add(name);
                                }
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

        public async Task Invoke(HttpContext context, Func<Task> next)
        {
            try
            {

                if (context.Request.Path.Value.Equals(_elmahRoot,StringComparison.InvariantCultureIgnoreCase) 
                    || context.Request.Path.Value.StartsWith(_elmahRoot+"/", StringComparison.InvariantCultureIgnoreCase))
                {

		            if (!_сheckPermissionAction(context))
		            {
			            await context.ChallengeAsync();
			            return;
		            }
                    await ProcessElamhRequest(context);
                    return;
                }

                await next();

                if (context.Response.HasStarted
                    || context.Response.StatusCode < 400
                    || context.Response.StatusCode >= 600
                    || context.Response.ContentLength.HasValue
                    || !string.IsNullOrEmpty(context.Response.ContentType))
                {
                   return;
                }
                await LogException(new HttpException(context.Response.StatusCode), context, _onError);
            }
            catch (Exception exception)
            {
                await LogException(exception, context, _onError);

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
                        await ProcessTemplate<ErrorDetailPage>(context, _errorLog, "text/html");
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
                        await StyleSheetHelper.LoadStyleSheets(context, StyleSheetHelper.StyleSheetResourceNames, "text/css",
                            Encoding.UTF8,
                            true);
                        break;
                    case "test":
                        throw new TestException();
                    case "about":
                        await ProcessTemplate<AboutPage>(context, _errorLog, "text/html");
                        break;
                    default:
                        await ProcessTemplate<ErrorLogPage>(context, _errorLog, "text/html");
                        break;
                }
            }
            catch (TestException)
            {
                throw;
            }
            catch(Exception)
            {
                _logger.LogError("Elmah request processing error");
            }
        }

        
        async Task ProcessTemplate<T>(HttpContext context, ErrorLog error, string contentType) where T : WebTemplateBase, new()
        {
            var template = new T { Context = context, ErrorLog = error, ElmahRoot = _elmahRoot};

            context.Response.ContentType = contentType;

            await context.Response.WriteAsync(template.TransformText());
        }


        internal async Task LogException(Exception e, HttpContext context, Func<HttpContext, Error, Task> onError)
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
                await onError(context, error);
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
            catch (Exception)
            {
                //
                // IMPORTANT! We swallow any exception raised during the 
                // logging and send them out to the trace . The idea 
                // here is that logging of exceptions by itself should not 
                // be  critical to the overall operation of the application.
                // The bad thing is that we catch ANY kind of exception, 
                // even system ones and potentially let them slip by.
                //

                _logger.LogError("Elmah local exception");
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
