using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ElmahCore
{
    /// <summary>
    ///     Elmah Options
    /// </summary>
    public class ElmahOptions
    {
        /// <summary>
        ///     ELMAH access url (default = '/elmah')
        /// </summary>
        internal PathString Path { get; set; }

        /// <summary>
        ///     Filters file path, example: options.LogPath = "~/elmah.xml"; // OR options.LogPath = "с:\elmah.xml"
        /// </summary>
        public string? FiltersConfig { get; set; }

        /// <summary>
        ///     Custom filters
        /// </summary>
        public ICollection<IErrorFilter> Filters { get; set; } = new List<IErrorFilter>();

        /// <summary>
        ///     Custom notifiers
        /// </summary>
        public ICollection<IErrorNotifier> Notifiers { get; set; } = new List<IErrorNotifier>();

        /// <summary>
        ///     Custom error handler
        /// </summary>
        public Func<HttpContext, Error, Task> OnError { get; set; } = (context, error) => Task.CompletedTask;

        /// <summary>
        ///     Application Name
        /// </summary>
        public string? ApplicationName { get; set; }

        /// <summary>
        ///     List of paths to sources
        /// </summary>
        public string[]? SourcePaths { get; set; }

        /// <summary>
        ///     Enable/Disable request body logging
        /// </summary>
        public bool LogRequestBody { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the Elmah error page when an error is raised.
        /// </summary>
        public bool ShowElmahErrorPage { get; set; }

        public virtual Task Error(HttpContext context, Error error)
        {
            return OnError(context, error);
        }
    }
}