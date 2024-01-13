using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Elmah.Xml;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Elmah.AspNetCore.MsSql;

/// <summary>
///     An <see cref="ErrorLog" /> implementation that uses MSSQL
///     as its backing store.
/// </summary>
// ReSharper disable once UnusedType.Global
public class SqlErrorLog : ErrorLog
{
    private const int MaxAppNameLength = 60;
    private volatile bool _checkTableExists = false;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlErrorLog" /> class
    ///     using a dictionary of configured settings.
    /// </summary>
    public SqlErrorLog(IOptions<SqlErrorLogOptions> option) 
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
    public SqlErrorLog(string connectionString, string? schemaName, string? tableName, bool createTablesIfNotExist)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        ConnectionString = connectionString;
        DatabaseSchemaName = !string.IsNullOrWhiteSpace(schemaName) ? schemaName! : "dbo";
        DatabaseTableName = !string.IsNullOrWhiteSpace(tableName) ? tableName! : "ELMAH_Error";

        _checkTableExists = createTablesIfNotExist;
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

    public override async Task LogAsync(Error error, CancellationToken cancellationToken)
    {
        try
        {
            await this.EnsureTablesExistAsync(cancellationToken);

            var errorXml = ErrorXml.EncodeString(error);

            using var connection = new SqlConnection(ConnectionString);
            using var command = Commands.LogError(error.Id, ApplicationName, error.HostName, error.Type, error.Source,
                       error.Message, error.User, error.StatusCode, error.Time, errorXml,
                       DatabaseSchemaName, DatabaseTableName);
            command.Connection = connection;
            await connection.OpenAsync(cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch
        {
            // guard: silently fail, this can't bubble up or it will create a stack overflow from errors attempting to log errors....
        }
    }

    public override async Task<ErrorLogEntry?> GetErrorAsync(Guid id, CancellationToken cancellationToken)
    {
        await this.EnsureTablesExistAsync(cancellationToken);

        string? errorXml;

        using (var connection = new SqlConnection(ConnectionString))
        using (var command = Commands.GetErrorXml(ApplicationName, id, DatabaseSchemaName, DatabaseTableName))
        {
            command.Connection = connection;
            await connection.OpenAsync(cancellationToken);
            errorXml = (string?)await command.ExecuteScalarAsync(cancellationToken);
        }

        if (errorXml == null)
        {
            return null;
        }

        var error = ErrorXml.DecodeString(id, errorXml);
        return new ErrorLogEntry(this, error);
    }

    public override async Task<int> GetErrorsAsync(ErrorLogFilterCollection filters, int errorIndex, int pageSize,
        ICollection<ErrorLogEntry> errorEntryList, CancellationToken cancellationToken)
    {
        if (errorIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(errorIndex), errorIndex, null);
        }

        if (pageSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, null);
        }

        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using (var command = Commands.GetErrorsXml(ApplicationName, errorIndex, pageSize, DatabaseSchemaName, DatabaseTableName))
        {
            command.Connection = connection;

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var id = reader.GetGuid(0);
                var xml = reader.GetString(1);
                var error = ErrorXml.DecodeString(id, xml);
                errorEntryList.Add(new ErrorLogEntry(this, error));
            }
        }

        using (var command = Commands.GetErrorsXmlTotal(ApplicationName, DatabaseSchemaName, DatabaseTableName))
        {
            command.Connection = connection;
            return (int)(await command.ExecuteScalarAsync(cancellationToken))!;
        }
    }

    /// <summary>
    ///  Creates the necessary tables and sequences used by this implementation
    /// </summary>
    private async Task EnsureTablesExistAsync(CancellationToken cancellationToken)
    {
        if (!_checkTableExists)
        {
            return;
        }

        _checkTableExists = false;

        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmdCheck = Commands.CheckTable(DatabaseSchemaName, DatabaseTableName);
        cmdCheck.Connection = connection;

        // ReSharper disable once PossibleNullReferenceException
        var exists = (int?)(await cmdCheck.ExecuteScalarAsync(cancellationToken));
        if (!exists.HasValue)
        {
            await ExecuteBatchNonQueryAsync(Commands.CreateTableSql(DatabaseSchemaName, DatabaseTableName), connection, cancellationToken);
        }
    }

    private async Task ExecuteBatchNonQueryAsync(string sql, SqlConnection conn, CancellationToken cancellationToken)
    {
        var sqlBatch = string.Empty;
        using var cmd = new SqlCommand(string.Empty, conn);

        sql += "\nGO"; // make sure last batch is executed.
        foreach (var line in sql.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.ToUpperInvariant().Trim() == "GO")
            {
                cmd.CommandText = sqlBatch;
                await cmd.ExecuteNonQueryAsync(cancellationToken);
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
/* elmah */
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

/* elmah */
ALTER TABLE [{schemaName}].[{tableName}] WITH NOCHECK ADD 
    CONSTRAINT [PK_{tableName}] PRIMARY KEY NONCLUSTERED ([ErrorId]) ON [PRIMARY] 
GO

/* elmah */
ALTER TABLE [{schemaName}].[{tableName}] ADD 
    CONSTRAINT [DF_{tableName}_ErrorId] DEFAULT (NEWID()) FOR [ErrorId]
GO

/* elmah */
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
            var command = new SqlCommand
            {
                CommandText = $@"
/* elmah */
SELECT 1 
WHERE EXISTS (
   SELECT 1
   FROM   INFORMATION_SCHEMA.TABLES 
   WHERE  TABLE_SCHEMA = '{schemaName}'
   AND    TABLE_NAME = '{tableName}'
   )
"
            };
            return command;
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
/* elmah */
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
/* elmah */
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
                CommandText = $"/* elmah */ SELECT COUNT(*) FROM [{schemaName}].[{tableName}] WHERE Application = @Application"
            };
            command.Parameters.Add("@Application", SqlDbType.NVarChar, MaxAppNameLength).Value = appName;
            return command;
        }
    }
}