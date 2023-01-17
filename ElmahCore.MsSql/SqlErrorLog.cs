using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace ElmahCore.Sql
{
    /// <summary>
    ///     An <see cref="ErrorLog" /> implementation that uses MSSQL
    ///     as its backing store.
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class SqlErrorLog : ErrorLog
    {
        private const int MaxAppNameLength = 60;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlErrorLog" /> class
        ///     using a dictionary of configured settings.
        /// </summary>
        public SqlErrorLog(IOptions<ElmahOptions> option) 
            : this(option.Value.ConnectionString, option.Value.SqlServerDatabaseSchemaName, option.Value.SqlServerDatabaseTableName, option.Value.CreateTablesIfNotExist)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlErrorLog" /> class
        ///     to use a specific connection string for connecting to the database.
        /// </summary>
        public SqlErrorLog(string connectionString)
            : this(connectionString, null, null, true)
        {

        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlErrorLog" /> class
        ///     to use a specific connection string for connecting to the database and a specific schema and table name.
        /// </summary>
        public SqlErrorLog(string connectionString, string schemaName, string tableName, bool createTablesIfNotExist)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
            DatabaseSchemaName = !string.IsNullOrWhiteSpace(schemaName) ? schemaName : "dbo";
            DatabaseTableName = !string.IsNullOrWhiteSpace(tableName) ? tableName : "ELMAH_Error";

            if (createTablesIfNotExist)
                CreateTableIfNotExists();
        }

        /// <summary>
        ///     Gets the name of this error log implementation.
        /// </summary>
        public override string Name => "MSSQL Error Log";

        /// <summary>
        ///     Gets the connection string used by the log to connect to the database.
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        public virtual string ConnectionString { get; }

        /// <summary>
        /// Gets the Schema name to be used for the error table
        /// </summary>
        public virtual string DatabaseSchemaName { get; }

        /// <summary>
        /// Gets the Table name to be used for the error table
        /// </summary>
        public virtual string DatabaseTableName { get; }

        public override string Log(Error error)
        {
            var id = Guid.NewGuid();
            Log(id, error);
            return id.ToString();
        }

        public override void Log(Guid id, Error error)
        {
            try
            {
                var errorXml = ErrorXml.EncodeString(error);

                using (var connection = new SqlConnection(ConnectionString))
                using (var command = Commands.LogError(id, ApplicationName, error.HostName, error.Type, error.Source,
                           error.Message, error.User, error.StatusCode, error.Time, errorXml,
                           DatabaseSchemaName, DatabaseTableName))
                {
                    command.Connection = connection;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                //guard: silently fail, this can't bubble up or it will create a stack overflow from errors attempting to log errors....
            }
        }

        public override ErrorLogEntry GetError(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (id.Length == 0) throw new ArgumentException(null, nameof(id));

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

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = Commands.GetErrorXml(ApplicationName, errorGuid,
                DatabaseSchemaName, DatabaseTableName))
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
            if (errorIndex < 0) throw new ArgumentOutOfRangeException(nameof(errorIndex), errorIndex, null);
            if (pageSize < 0) throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, null);

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = Commands.GetErrorsXml(ApplicationName, errorIndex, pageSize,
                DatabaseSchemaName, DatabaseTableName))
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

                using (var command = Commands.GetErrorsXmlTotal(ApplicationName,
                DatabaseSchemaName, DatabaseTableName))
                {
                    command.Connection = connection;
                    return int.Parse(command.ExecuteScalar().ToString());
                }
            }
        }

        /// <summary>
        ///  Creates the necessary tables and sequences used by this implementation
        /// </summary>
        private void CreateTableIfNotExists()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var cmdCheck = Commands.CheckTable(DatabaseSchemaName, DatabaseTableName))
                {
                    cmdCheck.Connection = connection;
                    // ReSharper disable once PossibleNullReferenceException
                    var exists = (int?) cmdCheck.ExecuteScalar();

                    if (!exists.HasValue) ExecuteBatchNonQuery(Commands.CreateTableSql(DatabaseSchemaName, DatabaseTableName), connection);
                }
            }
        }

        private void ExecuteBatchNonQuery(string sql, SqlConnection conn)
        {
            var sqlBatch = string.Empty;
            using (var cmd = new SqlCommand(string.Empty, conn))
            {
                sql += "\nGO"; // make sure last batch is executed.
                foreach (var line in sql.Split(new[] {"\n", "\r"},
                    StringSplitOptions.RemoveEmptyEntries))
                    if (line.ToUpperInvariant().Trim() == "GO")
                    {
                        cmd.CommandText = sqlBatch;
                        cmd.ExecuteNonQuery();
                        sqlBatch = string.Empty;
                    }
                    else
                    {
                        sqlBatch += line + "\n";
                    }
            }
        }

        private static class Commands
        {
            public static string CreateTableSql(string schemaName, string tableName)
            {
                return
                $@"
CREATE TABLE [{schemaName}].[{tableName}]
(
    [ErrorId]     UNIQUEIDENTIFIER NOT NULL,
    [Application] NVARCHAR(60)  NOT NULL,
    [Host]        NVARCHAR(50)  NOT NULL,
    [Type]        NVARCHAR(100) NOT NULL,
    [Source]      NVARCHAR(60)  NOT NULL,
    [Message]     NVARCHAR(MAX) NOT NULL,
    [User]        NVARCHAR(50)  NOT NULL,
    [StatusCode]  INT NOT NULL,
    [TimeUtc]     DATETIME NOT NULL,
    [Sequence]    INT IDENTITY (1, 1) NOT NULL,
    [AllXml]      NVARCHAR(MAX) NOT NULL 
) 
GO

ALTER TABLE [{schemaName}].[{tableName}] WITH NOCHECK ADD 
    CONSTRAINT [PK_{tableName}] PRIMARY KEY NONCLUSTERED ([ErrorId]) ON [PRIMARY] 
GO

ALTER TABLE [{schemaName}].[{tableName}] ADD 
    CONSTRAINT [DF_{tableName}_ErrorId] DEFAULT (NEWID()) FOR [ErrorId]
GO

CREATE NONCLUSTERED INDEX [IX_{tableName}_App_Time_Seq] ON [{schemaName}].[{tableName}] 
(
    [Application]   ASC,
    [TimeUtc]       DESC,
    [Sequence]      DESC
) 
ON [PRIMARY]";
            }

            public static SqlCommand CheckTable(string schemaName, string tableName)
            {
                return new SqlCommand
                {
                    CommandText = $@"
SELECT 1 
WHERE EXISTS (
   SELECT 1
   FROM   INFORMATION_SCHEMA.TABLES 
   WHERE  TABLE_SCHEMA = '{schemaName}'
   AND    TABLE_NAME = '{tableName}'
   )
"
                };
            }


            public static SqlCommand LogError(
                Guid id,
                string appName,
                string hostName,
                string typeName,
                string source,
                string message,
                string user,
                int statusCode,
                DateTime time,
                string xml,
                string schemaName, 
                string tableName)
            {
                var command = new SqlCommand
                {
                    CommandText = $@"
/* elmah */
INSERT INTO [{schemaName}].[{tableName}] (ErrorId, Application, Host, Type, Source, Message, ""User"", StatusCode, TimeUtc, AllXml)
VALUES (@ErrorId, @Application, @Host, @Type, @Source, @Message, @User, @StatusCode, @TimeUtc, @AllXml)
"
                };
                command.Parameters.Add(new SqlParameter("ErrorId", id));
                command.Parameters.Add(new SqlParameter("Application", appName));
                command.Parameters.Add(new SqlParameter("Host", hostName));
                command.Parameters.Add(new SqlParameter("Type", typeName));
                command.Parameters.Add(new SqlParameter("Source", source));
                command.Parameters.Add(new SqlParameter("Message", message));
                command.Parameters.Add(new SqlParameter("User", user));
                command.Parameters.Add(new SqlParameter("StatusCode", statusCode));
                command.Parameters.Add(new SqlParameter("TimeUtc", time.ToUniversalTime()));
                command.Parameters.Add(new SqlParameter("AllXml", xml));

                return command;
            }

            public static SqlCommand GetErrorXml(
                string appName,
                Guid id,
                string schemaName,
                string tableName)
            {
                var command = new SqlCommand
                {
                    CommandText = $@"
SELECT AllXml FROM [{schemaName}].[{tableName}]
WHERE 
    Application = @Application 
    AND ErrorId = @ErrorId
"
                };

                command.Parameters.Add(new SqlParameter("Application", appName));
                command.Parameters.Add(new SqlParameter("ErrorId", id));

                return command;
            }

            public static SqlCommand GetErrorsXml(
                string appName,
                int errorIndex,
                int pageSize,
                string schemaName,
                string tableName)
            {
                var command = new SqlCommand
                {
                    CommandText = $@"
SELECT ErrorId, AllXml FROM [{schemaName}].[{tableName}]
WHERE
    Application = @Application
ORDER BY [Sequence] DESC
OFFSET     @offset ROWS
FETCH NEXT @limit ROWS ONLY;
"
                };

                command.Parameters.Add("@Application", SqlDbType.NVarChar, MaxAppNameLength).Value = appName;
                command.Parameters.Add("@offset", SqlDbType.Int).Value = errorIndex;
                command.Parameters.Add("@limit", SqlDbType.Int).Value = pageSize;

                return command;
            }

            public static SqlCommand GetErrorsXmlTotal(string appName,
                string schemaName,
                string tableName)
            {
                var command = new SqlCommand
                {
                    CommandText = $"SELECT COUNT(*) FROM [{schemaName}].[{tableName}] WHERE Application = @Application"
                };
                command.Parameters.Add("@Application", SqlDbType.NVarChar, MaxAppNameLength).Value = appName;
                return command;
            }
        }
    }
}