using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ElmahCore
{
    public static class ErrorLogExtensions
    {
        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="Log(Error)" />. An additional parameter
        ///     specifies a <see cref="CancellationToken" /> to use.
        /// </summary>
        public static Task<string> LogAsync(this ErrorLog log, Error error) => log.LogAsync(error, default);

        /// <summary>
        ///     When overridden in a subclass, begins an asynchronous version
        ///     of <see cref="Log(Error)" />.
        /// </summary>
        public static IAsyncResult BeginLog(this ErrorLog log, Error error, AsyncCallback asyncCallback, object asyncState)
            => log.LogAsync(error, default).Apmize(asyncCallback, asyncState);

        /// <summary>
        ///     When overridden in a subclass, ends an asynchronous version
        ///     of <see cref="Log(Error)" />.
        /// </summary>
        public static string EndLog(IAsyncResult asyncResult) => EndApmizedTask<string>(asyncResult);

        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="GetError" />.
        /// </summary>
        public static Task<ErrorLogEntry?> GetErrorAsync(this ErrorLog log, string id) => log.GetErrorAsync(id, default);

        /// <summary>
        ///     When overridden in a subclass, begins an asynchronous version
        ///     of <see cref="GetError" />.
        /// </summary>
        public static IAsyncResult BeginGetError(this ErrorLog log, string id, AsyncCallback asyncCallback, object asyncState)
            => log.GetErrorAsync(id, default).Apmize(asyncCallback, asyncState);

        /// <summary>
        ///     When overridden in a subclass, ends an asynchronous version
        ///     of <see cref="GetError" />.
        /// </summary>
        public static ErrorLogEntry EndGetError(this ErrorLog log, IAsyncResult asyncResult) => EndApmizedTask<ErrorLogEntry>(asyncResult);

        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="GetErrors" />. An additional
        ///     parameter specifies a <see cref="CancellationToken" /> to use.
        /// </summary>
        public static Task<int> GetErrorsAsync(this ErrorLog log, string? searchText, List<ErrorLogFilter> errorLogFilters, int errorIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList)
            => log.GetErrorsAsync(searchText, errorLogFilters, errorIndex, pageSize, errorEntryList, default);

        /// <summary>
        ///     When overridden in a subclass, begins an asynchronous version
        ///     of <see cref="GetErrors" />.
        /// </summary>
        public static IAsyncResult BeginGetErrors(this ErrorLog log, string searchText, List<ErrorLogFilter> errorLogFilters, int pageIndex, int pageSize,
            ICollection<ErrorLogEntry> errorEntryList, AsyncCallback asyncCallback, object asyncState)
        {
            return log.GetErrorsAsync(searchText, errorLogFilters, pageIndex, pageSize, errorEntryList, default)
                .Apmize(asyncCallback, asyncState);
        }

        /// <summary>
        ///     When overridden in a subclass, ends an asynchronous version
        ///     of <see cref="GetErrors" />.
        /// </summary>
        public static int EndGetErrors(this ErrorLog log, IAsyncResult asyncResult)
        {
            return EndApmizedTask<int>(asyncResult);
        }

        private static T EndApmizedTask<T>(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            var task = asyncResult as Task<T> ?? throw new ArgumentException(null, nameof(asyncResult));
            try
            {
                return task.Result;
            }
            catch (AggregateException e)
            {
                // ReSharper disable once PossibleNullReferenceException
                throw e.InnerException; // TODO handle stack trace reset?
            }
        }
    }
}
