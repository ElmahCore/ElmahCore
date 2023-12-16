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
        private string? _appName;
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
            get => _appName ?? Assembly.GetEntryAssembly()?.GetName().Name!;

            set
            {
                if (_appNameInitialized)
                {
                    throw new InvalidOperationException("The application name cannot be reset once initialized.");
                }

                _appName = value;
                _appNameInitialized = (value ?? string.Empty).Length > 0;
            }
        }

        public string[]? SourcePaths { get; set; }

        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="Log(Error)" />. An additional parameter
        ///     specifies a <see cref="CancellationToken" /> to use.
        /// </summary>
        public virtual async Task<string> LogAsync(Error error, CancellationToken cancellationToken)
        {
            Guid id = Guid.NewGuid();
            await this.LogAsync(id, error, cancellationToken);
            return id.ToString();
        }

        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="Log(Error)" />. An additional parameter
        ///     specifies a <see cref="CancellationToken" /> to use.
        /// </summary>
        public abstract Task LogAsync(Guid id, Error error, CancellationToken cancellationToken);

        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="GetError" />. An additional parameter
        ///     specifies a <see cref="CancellationToken" /> to use.
        /// </summary>
        // ReSharper disable once UnusedParameter.Global
        public abstract Task<ErrorLogEntry?> GetErrorAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        ///     When overridden in a subclass, starts a task that asynchronously
        ///     does the same as <see cref="GetErrors" />.
        /// </summary>
        public abstract Task<int> GetErrorsAsync(string? searchText, List<ErrorLogFilter> errorLogFilters, int errorIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList,
            CancellationToken cancellationToken);

        public async Task<int> GetNewErrorsAsync(string? searchText, List<ErrorLogFilter> errorLogFilters, string id, List<ErrorLogEntry> entries, CancellationToken cancellationToken)
        {
            int cnt = 0, count, page = 0;
            do
            {
                var errors = new List<ErrorLogEntry>();
                count = await GetErrorsAsync(searchText, errorLogFilters, page, 10, errors, cancellationToken);
                foreach (var el in errors)
                {
                    if (el.Id == id)
                    {
                        return cnt;
                    }

                    cnt += 1;
                    entries.Add(el);
                }

                page += 1;
            } while (cnt > 0);

            return count;
        }
    }
}