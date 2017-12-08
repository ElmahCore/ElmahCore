#region License, Terms and Author(s)
//
// ELMAH - Error Logging Modules and Handlers for ASP.NET
// Copyright (c) 2004-9 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mannex.Threading.Tasks;
using Microsoft.Extensions.PlatformAbstractions;

//[assembly: Elmah.Scc("$Id: ErrorLog.cs 776 2011-01-12 21:09:24Z azizatif $")]

namespace ElmahCore
{

    using System.Threading;
    using System.Threading.Tasks;
    using Mannex.Threading.Tasks;

    /// <summary>
    /// Represents an error log capable of storing and retrieving errors
    /// generated in an ASP.NET Web application.
    /// </summary>

    public abstract class ErrorLog
    {
        private string _appName;
        private bool _appNameInitialized;
        private static readonly object _contextKey = new object();

        /// <summary>
        /// Logs an error in log for the application.
        /// </summary>
        
        public abstract string Log(Error error);

        protected ErrorLog()
        {
            ApplicationName = $"{PlatformServices.Default.Application.ApplicationName}({PlatformServices.Default.Application.ApplicationVersion})";

        }

        /// <summary>
        /// When overridden in a subclass, starts a task that asynchronously
        /// does the same as <see cref="Log"/>.
        /// </summary>

        public Task<string> LogAsync(Error error)
        {
            return LogAsync(error, CancellationToken.None);
        }

        /// <summary>
        /// When overridden in a subclass, starts a task that asynchronously
        /// does the same as <see cref="Log"/>. An additional parameter
        /// specifies a <see cref="CancellationToken"/> to use.
        /// </summary>

        public virtual Task<string> LogAsync(Error error, CancellationToken cancellationToken)
        {
            return Async.RunSynchronously(Log, error);
        }
        

        /// <summary>
        /// When overridden in a subclass, begins an asynchronous version 
        /// of <see cref="Log"/>.
        /// </summary>

        public virtual IAsyncResult BeginLog(Error error, AsyncCallback asyncCallback, object asyncState)
        {
            return LogAsync(error, CancellationToken.None).Apmize(asyncCallback, asyncState);
        }

        /// <summary>
        /// When overridden in a subclass, ends an asynchronous version 
        /// of <see cref="Log"/>.
        /// </summary>

        public virtual string EndLog(IAsyncResult asyncResult)
        {
            return EndApmizedTask<string>(asyncResult);
        }

        /// <summary>
        /// Retrieves a single application error from log given its 
        /// identifier, or null if it does not exist.
        /// </summary>

        public abstract ErrorLogEntry GetError(string id);


        /// <summary>
        /// When overridden in a subclass, starts a task that asynchronously
        /// does the same as <see cref="GetError"/>.
        /// </summary>

        public Task<ErrorLogEntry> GetErrorAsync(string id)
        {
            return GetErrorAsync(id, CancellationToken.None);
        }

        /// <summary>
        /// When overridden in a subclass, starts a task that asynchronously
        /// does the same as <see cref="GetError"/>. An additional parameter
        /// specifies a <see cref="CancellationToken"/> to use.
        /// </summary>

        public virtual Task<ErrorLogEntry> GetErrorAsync(string id, CancellationToken cancellationToken)
        {
            return Async.RunSynchronously(GetError, id);
        }


        /// <summary>
        /// When overridden in a subclass, begins an asynchronous version 
        /// of <see cref="GetError"/>.
        /// </summary>

        public virtual IAsyncResult BeginGetError(string id, AsyncCallback asyncCallback, object asyncState)
        {
            return GetErrorAsync(id, CancellationToken.None).Apmize(asyncCallback, asyncState);
        }

        /// <summary>
        /// When overridden in a subclass, ends an asynchronous version 
        /// of <see cref="GetError"/>.
        /// </summary>

        public virtual ErrorLogEntry EndGetError(IAsyncResult asyncResult)
        {
            return EndApmizedTask<ErrorLogEntry>(asyncResult);
        }

        /// <summary>
        /// Retrieves a page of application errors from the log in 
        /// descending order of logged time.
        /// </summary>

        public abstract int GetErrors(int pageIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList);


        /// <summary>
        /// When overridden in a subclass, starts a task that asynchronously
        /// does the same as <see cref="GetErrors"/>. An additional 
        /// parameter specifies a <see cref="CancellationToken"/> to use.
        /// </summary>

        public Task<int> GetErrorsAsync(int pageIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList)
        {
            return GetErrorsAsync(pageIndex, pageSize, errorEntryList, CancellationToken.None);
        }

        /// <summary>
        /// When overridden in a subclass, starts a task that asynchronously
        /// does the same as <see cref="GetErrors"/>.
        /// </summary>

        public virtual Task<int> GetErrorsAsync(int pageIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList, CancellationToken cancellationToken)
        {
            return Async.RunSynchronously(GetErrors, pageIndex, pageSize, errorEntryList);
        }


        /// <summary>
        /// When overridden in a subclass, begins an asynchronous version 
        /// of <see cref="GetErrors"/>.
        /// </summary>

        public virtual IAsyncResult BeginGetErrors(int pageIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList, AsyncCallback asyncCallback, object asyncState)
        {
            return GetErrorsAsync(pageIndex, pageSize, errorEntryList, CancellationToken.None).Apmize(asyncCallback, asyncState);
        }

        /// <summary>
        /// When overridden in a subclass, ends an asynchronous version 
        /// of <see cref="GetErrors"/>.
        /// </summary>
        
        public virtual int EndGetErrors(IAsyncResult asyncResult)
        {
            return EndApmizedTask<int>(asyncResult);
        }

        /// <summary>
        /// Get the name of this log.
        /// </summary>

        public virtual string Name
        {
            get { return GetType().Name; }   
        }

        /// <summary>
        /// Gets the name of the application to which the log is scoped.
        /// </summary>
        
        public string ApplicationName
        {
            get { return _appName ?? string.Empty; }
            
            set
            {
                if (_appNameInitialized)
                    throw new InvalidOperationException("The application name cannot be reset once initialized.");

                _appName = value;
                _appNameInitialized = (value ?? string.Empty).Length > 0;
            }
        }


        static T EndApmizedTask<T>(IAsyncResult asyncResult)
        {
            if (asyncResult == null) throw new ArgumentNullException(nameof(asyncResult));
            var task = asyncResult as Task<T>;
            if (task == null) throw new ArgumentException(null, "asyncResult");
            try
            {
                return task.Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException; // TODO handle stack trace reset?
            }
        }

    }
}
