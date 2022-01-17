using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace ElmahCore.MySql
{
    /// <summary>
    ///     An <see cref="ErrorLog" /> implementation that uses MySQL
    ///     as its backing store.
    /// </summary>
    public class MySqlErrorLog : ErrorLog
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MySqlErrorLog" /> class
        ///     using a dictionary of configured settings.
        /// </summary>
        public MySqlErrorLog(IOptions<ElmahOptions> option) : this(option.Value.ConnectionString)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MySqlErrorLog" /> class
        ///     to use a specific connection string for connecting to the database.
        /// </summary>
        public MySqlErrorLog(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            ConnectionString = connectionString;
            CreateTableIfNotExist();
        }

        /// <summary>
        ///     Gets the name of this error log implementation.
        /// </summary>
        public override string Name => "MySQL Error Log";

        /// <summary>
        ///     Gets the connection string used by the log to connect to the database.
        /// </summary>
        public virtual string ConnectionString { get; }

        public override string Log(Error error)
        {
            var id = Guid.NewGuid();

            Log(id, error);

            return id.ToString();
        }

        public override void Log(Guid id, Error error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            var errorXml = ErrorXml.EncodeString(error);

            using (var connection = new MySqlConnection(ConnectionString))
            using (var command = CommandExtension.LogError(id, ApplicationName, error.HostName, error.Type,
                error.Source, error.Message, error.User, error.StatusCode, error.Time, errorXml))
            {
                connection.Open();
                command.Connection = connection;
                command.ExecuteNonQuery();
            }
        }

        public override ErrorLogEntry GetError(string id)
        {
            if (id == null) throw new ArgumentNullException("id");
            if (id.Length == 0) throw new ArgumentException(null, "id");

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

            using (var connection = new MySqlConnection(ConnectionString))
            using (var command = CommandExtension.GetErrorXml(ApplicationName, errorGuid))
            {
                command.Connection = connection;
                connection.Open();
                errorXml = (string) command.ExecuteScalar();
            }

            if (errorXml == null)
                return null;

            var error = ErrorXml.DecodeString(errorXml);
            return new ErrorLogEntry(this, id, error);
        }

        public override int GetErrors(int errorIndex, int pageSize, ICollection<ErrorLogEntry> errorEntryList)
        {
            if (errorIndex < 0) throw new ArgumentOutOfRangeException("errorIndex", errorIndex, null);
            if (pageSize < 0) throw new ArgumentOutOfRangeException("pageSize", pageSize, null);

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = CommandExtension.GetErrorsXml(ApplicationName, errorIndex, pageSize))
                {
                    command.Connection = connection;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetGuid(0);
                            var xml = reader.GetString(1);
                            var error = ErrorXml.DecodeString(xml);
                            errorEntryList.Add(new ErrorLogEntry(this, id.ToString(), error));
                        }
                    }
                }

                return GetTotalErrorsXml(connection);
            }
        }

        /// <summary>
        ///     Creates the neccessary tables used by this implementation
        /// </summary>
        private void CreateTableIfNotExist()
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var databaseName = connection.Database;

                using (var commandCheck = CommandExtension.CheckTable(databaseName))
                {
                    commandCheck.Connection = connection;
                    var exists = (int) commandCheck.ExecuteScalar() == 1;

                    if (!exists)
                        using (var commandCreate = CommandExtension.CreateTable())
                        {
                            commandCreate.Connection = connection;
                            commandCreate.ExecuteNonQuery();
                        }
                }
            }
        }

        private int GetTotalErrorsXml(MySqlConnection connection)
        {
            using (var command = CommandExtension.GetTotalErrorsXml(ApplicationName))
            {
                command.Connection = connection;
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}