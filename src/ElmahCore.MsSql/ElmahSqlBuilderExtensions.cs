using ElmahCore.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace ElmahCore;

public static class ElmahSqlBuilderExtensions
{
    public static void PersistToSql(this IElmahBuilder builder, Action<SqlErrorLogOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.PersistTo(provider => new SqlErrorLog(provider.GetRequiredService<IOptions<SqlErrorLogOptions>>()));
    }
}
