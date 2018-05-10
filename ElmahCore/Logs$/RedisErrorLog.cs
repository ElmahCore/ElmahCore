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

#define ASYNC_ADONET

using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace ElmahCore
{
    using ServiceStack.Redis;
    #region Imports

    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Xml;

    using IDictionary = System.Collections.IDictionary;
    using IList = System.Collections.IList;

    #endregion

    /// <summary>
    /// An <see cref="ErrorLog"/> implementation that uses Microsoft SQL 
    /// Server 2000 as its backing store.
    /// </summary>

    public class RedisErrorLog : ErrorLog
    {
        private readonly string _connectionString;
        private const int _maxAppNameLength = 60;
        private delegate RV Function<RV, A>(A a);
        public RedisErrorLog(IOptions<ElmahOptions> option) : this(option.Value.ConnectionString)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlErrorLog"/> class
        /// to use a specific connection string for connecting to the database.
        /// </summary>
        public RedisErrorLog(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");

            if (connectionString.Length == 0)
                throw new ArgumentException(null, "connectionString");

            _connectionString = connectionString;
        }
        /// <summary>
        /// Gets the name of this error log implementation.
        /// </summary>
        public override string Name
        {
            get { return "Error Log based on Redis Cache Server"; }
        }
        /// <summary>
        /// Gets the connection string used by the log to connect to the database.
        /// </summary>
        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }
        /// <summary>
        /// Logs an error to the database.
        /// </summary>
        /// <remarks>
        /// Use the stored procedure called by this implementation to set a
        /// policy on how long errors are kept in the log. The default
        /// implementation stores all errors for an indefinite time.
        /// </remarks>
        public override string Log(Error error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            string errorXml = ErrorXml.EncodeString(error);
            Guid id = Guid.NewGuid();

            var redisManager = new RedisManagerPool(this.ConnectionString);
            using (var client = redisManager.GetClient())
            {
                client.As<RedisObject>().Store(new RedisObject()
                {
                    AllXml = errorXml,
                    Application = this.ApplicationName,
                    ErrorId = id,
                    Host = error.HostName,
                    Message = error.Message,
                    Source = error.Source,
                    StatusCode = error.StatusCode,
                    TimeUtc = error.Time.ToUniversalTime(),
                    Type = error.Type,
                    User = error.User
                });
            }
            return id.ToString();
        }
        /// <summary>
        /// Returns a page of errors from the databse in descending order 
        /// of logged time.
        /// </summary>
        public override int GetErrors(int pageIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException("pageIndex", pageIndex, null);
            if (pageSize < 0)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, null);
            var redisManager = new RedisManagerPool(this.ConnectionString);
            using (var client = redisManager.GetClient())
            {
                var objects = client.As<RedisObject>().GetAll().Take(pageSize).Skip(pageIndex * pageSize);
                foreach (var redisError in objects)
                {
                    errorEntryList.Add(new ErrorLogEntry(this, redisError.ErrorId.ToString(), ErrorXml.DecodeString(redisError.AllXml)));
                }
                var total = client.As<RedisObject>().GetAll().Count();
                return total;
            }
        }
        private void ErrorsXmlToList(XmlReader reader, ICollection<ErrorLogEntry> errorEntryList)
        {
            Debug.Assert(reader != null);

            if (errorEntryList != null)
            {
                while (reader.IsStartElement("error"))
                {
                    string id = reader.GetAttribute("errorId");
                    Error error = ErrorXml.Decode(reader);
                    errorEntryList.Add(new ErrorLogEntry(this, id, error));
                }
            }
        }
        /// <summary>
        /// Returns the specified error from the database, or null 
        /// if it does not exist.
        /// </summary>
        public override ErrorLogEntry GetError(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (id.Length == 0)
                throw new ArgumentException(null, "id");
            Guid errorGuid;
            try
            {
                errorGuid = new Guid(id);
            }
            catch (FormatException e)
            {
                throw new ArgumentException(e.Message, "id", e);
            }
            string errorXml;
            var redisManager = new RedisManagerPool(this.ConnectionString);
            using (var client = redisManager.GetClient())
            {
                errorXml = client.As<RedisObject>().GetAll().FirstOrDefault(x => x.ErrorId == errorGuid).AllXml;
            }
            //using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            //using (SqlCommand command = Commands.GetErrorXml(this.ApplicationName, errorGuid))
            //{
            //    command.Connection = connection;
            //    connection.Open();
            //    errorXml = (string)command.ExecuteScalar();
            //}

            if (errorXml == null)
                return null;

            Error error = ErrorXml.DecodeString(errorXml);
            return new ErrorLogEntry(this, id, error);
        }
        /// <summary>
        /// An <see cref="IAsyncResult"/> implementation that wraps another.
        /// </summary>
        private sealed class AsyncResultWrapper : IAsyncResult
        {
            private readonly IAsyncResult _inner;
            private readonly object _asyncState;

            public AsyncResultWrapper(IAsyncResult inner, object asyncState)
            {
                _inner = inner;
                _asyncState = asyncState;
            }

            public IAsyncResult InnerResult
            {
                get { return _inner; }
            }

            public bool IsCompleted
            {
                get { return _inner.IsCompleted; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { return _inner.AsyncWaitHandle; }
            }

            public object AsyncState
            {
                get { return _asyncState; }
            }

            public bool CompletedSynchronously
            {
                get { return _inner.CompletedSynchronously; }
            }
        }
        public class RedisObject
        {
            public Guid ErrorId { get; set; }
            public string Application { get; set; }
            public string Host { get; set; }
            public string Type { get; set; }
            public string Source { get; set; }
            public string Message { get; set; }
            public string User { get; set; }
            public string AllXml { get; set; }
            public int StatusCode { get; set; }
            public DateTime TimeUtc { get; set; }
        }
    }
}
