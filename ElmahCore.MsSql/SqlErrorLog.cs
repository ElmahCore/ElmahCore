using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml;
using Microsoft.Extensions.Options;

namespace ElmahCore.Sql
{

	/// <inheritdoc />
	/// <summary>
	/// An <see cref="T:ElmahCore.ErrorLog" /> implementation that uses Microsoft SQL 
	/// Server 2000 as its backing store.
	/// </summary>

	// ReSharper disable once UnusedMember.Global
	public class SqlErrorLog : ErrorLog
    {

	    // ReSharper disable once UnusedMember.Global
	    public SqlErrorLog(IOptions<ElmahOptions> option) : this(option.Value.ConnectionString)
        {

        }


        /// <summary>
            /// Initializes a new instance of the <see cref="SqlErrorLog"/> class
            /// to use a specific connection string for connecting to the database.
            /// </summary>

            public SqlErrorLog(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));

            if (connectionString.Length == 0)
                throw new ArgumentException(null, nameof(connectionString));
            
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets the name of this error log implementation.
        /// </summary>
        
        public override string Name => "Microsoft SQL Server Error Log";

	    /// <summary>
        /// Gets the connection string used by the log to connect to the database.
        /// </summary>
        
        public virtual string ConnectionString { get; }

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
                throw new ArgumentNullException(nameof(error));

            var errorXml = ErrorXml.EncodeString(error);
            var id = Guid.NewGuid();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = CommandExtension.LogError(
                id, ApplicationName, 
                error.HostName, error.Type, error.Source, error.Message, error.User,
                error.StatusCode, error.Time.ToUniversalTime(), errorXml))
            {
                command.Connection = connection;
                connection.Open();
                command.ExecuteNonQuery();
                return id.ToString();
            }
        }

        /// <summary>
        /// Returns a page of errors from the database in descending order 
        /// of logged time.
        /// </summary>

        public override int GetErrors(int pageIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), pageIndex, null);

            if (pageSize < 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, null);

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = CommandExtension.GetErrorsXml(ApplicationName, pageIndex, pageSize))
            {
                command.Connection = connection;
                connection.Open();

                XmlReader reader = command.ExecuteXmlReader();
                try
                {
                    ErrorsXmlToList(reader, errorEntryList);
                }
                finally
                {
                    reader.Close();
                }

	            CommandExtension.GetErrorsXmlOutputs(command, out var total);
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
                throw new ArgumentNullException(nameof(id));

            if (id.Length == 0)
                throw new ArgumentException(null, nameof(id));

            Guid errorGuid;

            try
            {
                errorGuid = new Guid(id);
            }
            catch (FormatException e)
            {
                throw new ArgumentException(e.Message, nameof(id), e);
            }

            string errorXml;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = CommandExtension.GetErrorXml(ApplicationName, errorGuid))
            {
                command.Connection = connection;
                connection.Open();
                errorXml = (string) command.ExecuteScalar();
            }

            if (errorXml == null)
                return null;

            Error error = ErrorXml.DecodeString(errorXml);
            return new ErrorLogEntry(this, id, error);
        }
    }
}
