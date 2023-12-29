namespace Elmah.AspNetCore.Postgresql;

public class PgsqlErrorLogOptions
{
    /// <summary>
    ///     Database connection string.
    /// </summary>
    public string ConnectionString { get; set; } = default!;

    /// <summary>
    ///     Database table name.
    ///     Default value if not provided = "ELMAH_Error"
    /// </summary>
    public string SqlServerDatabaseTableName { get; set; } = "ELMAH_Error";

    /// <summary>
    ///     Database schema name.
    ///     Default value if not provided = "dbo"
    /// </summary>
    public string SqlServerDatabaseSchemaName { get; set; } = "dbo";

    /// <summary>
    /// Indicate if the CreateTables check should be run when initializing the logger.
    /// Defaults to true
    /// </summary>
    public bool CreateTablesIfNotExist { get; set; } = true;
}
