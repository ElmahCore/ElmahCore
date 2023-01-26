using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedParameter.Global

namespace ElmahCore
{
    /// <summary>
    ///     Represents an error log capable of storing and retrieving errors
    ///     generated in an ASP.NET Web application.
    /// </summary>
    public abstract class ErrorLog
    {
        private string _appName;
        private bool _appNameInitialized;

        /// <summary>
        ///     Get the name of this log.
        /// </summary>

        public virtual string Name => GetType().Name;

        /// <summary>
        ///     Gets the name of the application to which the log is scoped.
        /// </summary>

        public string ApplicationName
        {
            get => _appName ?? Assembly.GetEntryAssembly()?.GetName().Name;

            set
            {
                if (_appNameInitialized)
                    throw new InvalidOperationException("The application name cannot be reset once initialized.");

                _appName = value;
                _appNameInitialized = (value ?? string.Empty).Length > 0;
            }
        }

        public string[] SourcePaths { get; set; }

        /// <summary>
        ///     Logs an error in log for the application.
        /// </summary>
        public abstract string Log(Error error);

        public abstract void Log(Guid id, Error error);

        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="Log(Error)" />.
        /// </summary>
        public Task<string> LogAsync(Error error)
        {
            return LogAsync(error, CancellationToken.None);
        }

        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="Log(Error)" />. An additional parameter
        ///     specifies a <see cref="CancellationToken" /> to use.
        /// </summary>
        public virtual Task<string> LogAsync(Error error, CancellationToken cancellationToken)
        {
            return Task.FromResult(Log(error));
        }


        /// <summary>
        ///     When overridden in a subclass, begins an asynchronous version
        ///     of <see cref="Log(Error)" />.
        /// </summary>
        public virtual IAsyncResult BeginLog(Error error, AsyncCallback asyncCallback, object asyncState)
        {
            return LogAsync(error, CancellationToken.None).Apmize(asyncCallback, asyncState);
        }

        /// <summary>
        ///     When overridden in a subclass, ends an asynchronous version
        ///     of <see cref="Log(Error)" />.
        /// </summary>
        public virtual string EndLog(IAsyncResult asyncResult)
        {
            return EndApmizedTask<string>(asyncResult);
        }

        /// <summary>
        ///     Retrieves a single application error from log given its
        ///     identifier, or null if it does not exist.
        /// </summary>
        public abstract ErrorLogEntry GetError(string id);


        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="GetError" />.
        /// </summary>
        public Task<ErrorLogEntry> GetErrorAsync(string id)
        {
            return GetErrorAsync(id, CancellationToken.None);
        }

        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="GetError" />. An additional parameter
        ///     specifies a <see cref="CancellationToken" /> to use.
        /// </summary>

        // ReSharper disable once UnusedParameter.Global
        public virtual Task<ErrorLogEntry> GetErrorAsync(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult(GetError(id));
        }


        /// <summary>
        ///     When overridden in a subclass, begins an asynchronous version
        ///     of <see cref="GetError" />.
        /// </summary>
        public virtual IAsyncResult BeginGetError(string id, AsyncCallback asyncCallback, object asyncState)
        {
            return GetErrorAsync(id, CancellationToken.None).Apmize(asyncCallback, asyncState);
        }

        /// <summary>
        ///     When overridden in a subclass, ends an asynchronous version
        ///     of <see cref="GetError" />.
        /// </summary>
        public virtual ErrorLogEntry EndGetError(IAsyncResult asyncResult)
        {
            return EndApmizedTask<ErrorLogEntry>(asyncResult);
        }

        /// <summary>
        ///     Retrieves a page of application errors from the log in
        ///     descending order of logged time.
        /// </summary>
        public abstract int GetErrors(string searchText, List<ErrorLogFilter> filters, int errorIndex, int pageSize,
            ICollection<ErrorLogEntry> errorEntryList);


        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="GetErrors" />. An additional
        ///     parameter specifies a <see cref="CancellationToken" /> to use.
        /// </summary>
        public Task<int> GetErrorsAsync(string searchText, List<ErrorLogFilter> errorLogFilters, int errorIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList)
        {
            return GetErrorsAsync(searchText, errorLogFilters, errorIndex, pageSize, errorEntryList, CancellationToken.None);
        }

        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="GetErrors" />.
        /// </summary>
        public virtual Task<int> GetErrorsAsync(string searchText, List<ErrorLogFilter> errorLogFilters, int errorIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(GetErrors(searchText, errorLogFilters, errorIndex, pageSize, errorEntryList));
        }


        /// <summary>
        ///     When overridden in a subclass, begins an asynchronous version
        ///     of <see cref="GetErrors" />.
        /// </summary>
        public virtual IAsyncResult BeginGetErrors(string searchText, List<ErrorLogFilter> errorLogFilters, int pageIndex, int pageSize,
            ICollection<ErrorLogEntry> errorEntryList, AsyncCallback asyncCallback, object asyncState)
        {
            return GetErrorsAsync(searchText, errorLogFilters, pageIndex, pageSize, errorEntryList, CancellationToken.None)
                .Apmize(asyncCallback, asyncState);
        }

        /// <summary>
        ///     When overridden in a subclass, ends an asynchronous version
        ///     of <see cref="GetErrors" />.
        /// </summary>
        public virtual int EndGetErrors(IAsyncResult asyncResult)
        {
            return EndApmizedTask<int>(asyncResult);
        }


        private static T EndApmizedTask<T>(IAsyncResult asyncResult)
        {
            if (asyncResult == null) throw new ArgumentNullException(nameof(asyncResult));
            var task = asyncResult as Task<T>;
            if (task == null) throw new ArgumentException(null, nameof(asyncResult));
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

        public async Task<int> GetNewErrorsAsync(string searchText, List<ErrorLogFilter> errorLogFilters, string id, List<ErrorLogEntry> entries)
        {
            int cnt = 0, count, page = 0;
            do
            {
                var errors = new List<ErrorLogEntry>();
                count = await GetErrorsAsync(searchText, errorLogFilters, page, 10, errors);
                foreach (var el in errors)
                {
                    if (el.Id == id) return cnt;
                    cnt += 1;
                    entries.Add(el);
                }
                page += 1;
            } while (cnt > 0);

            return count;
        }
    }
}