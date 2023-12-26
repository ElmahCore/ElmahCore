using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using ElmahCore.Assertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace ElmahCore;

internal sealed class ElmahExceptionLogger : IElmahExceptionLogger
{
    private readonly ErrorLog _errorLog;
    private readonly ILogger<ElmahExceptionLogger> _logger;
    private readonly List<IErrorFilter> _filters = new List<IErrorFilter>();
    private readonly IEnumerable<IErrorNotifier> _notifiers = Enumerable.Empty<IErrorNotifier>();
    private readonly Func<HttpContext, Error, Task> _onError = (context, error) => Task.CompletedTask;

    public ElmahExceptionLogger(ErrorLog errorLog, IOptions<ElmahOptions> elmahOptions, ILogger<ElmahExceptionLogger> logger)
    {
        _errorLog = errorLog;
        _logger = logger;

        var options = elmahOptions.Value;
        _onError = options.Error;

        //Notifiers
        if (options.Notifiers != null)
        {
            _notifiers = elmahOptions.Value.Notifiers.ToList();
        }

        //Filters
        _filters = options.Filters.ToList();
        
        if (!string.IsNullOrEmpty(options.FiltersConfig))
        {
            try
            {
                ConfigureFilters(options.FiltersConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in filters XML file");
            }
        }
    }

    public async Task<ErrorLogEntry?> LogExceptionAsync(HttpContext context, Exception e, string? body = null)
    {
        ArgumentNullException.ThrowIfNull(e);

        //
        // Fire an event to check if listeners want to filter out
        // logging of the uncaught exception.
        //
        ErrorLogEntry? entry = null;

        try
        {
            var args = new ExceptionFilterEventArgs(e, context);
            if (_filters.Any())
            {
                OnFiltering(args);

                if (args.Dismissed && !args.DismissedNotifiers.Any())
                {
                    return null;
                }
            }

            //
            // AddMessage away...
            //
            var error = new Error(e, context, body);

            await _onError(context, error);
            var log = _errorLog;
            error.ApplicationName = log.ApplicationName;
            await log.LogAsync(error);
            entry = new ErrorLogEntry(log, error);

            //Send notification
            foreach (var notifier in _notifiers)
            {
                if (!args.DismissedNotifiers.Any(i =>
                        i.Equals(notifier.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    notifier.Notify(error);
                }
            }
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

        return entry;
    }

    private void ConfigureFilters(string config)
    {
        var doc = new XmlDocument();
        doc.Load(config);
        var filterNodes = doc.SelectNodes("//errorFilter");
        if (filterNodes != null)
        {
            foreach (XmlNode filterNode in filterNodes)
            {
                var notList = new List<string>();
                var notifiers = filterNode.SelectNodes("//notifier");
                {
                    if (notifiers != null)
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
                }
                var assertionNode = filterNode.SelectSingleNode("test/*") as XmlElement;
                if (assertionNode != null)
                {
                    var a = AssertionFactory.Create(assertionNode);
                    var filter = new ErrorFilter(a, notList);
                    _filters.Add(filter);
                }
            }
        }
    }

    /// <summary>
    ///     Raises the <see cref="Filtering" /> event.
    /// </summary>
    private void OnFiltering(ExceptionFilterEventArgs args)
    {
        foreach (var filter in _filters)
        {
            filter.OnErrorModuleFiltering(this, args);
        }
    }
}