namespace Elmah.AspNetCore.MySql;

public class MySqlErrorLogOptions
{
    /// <summary>
    ///     Database connection string.
    /// </summary>
    public string ConnectionString { get; set; } = default!;
}
