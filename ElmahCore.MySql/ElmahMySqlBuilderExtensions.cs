using System;
using ElmahCore.MySql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ElmahCore;

public static class ElmahMySqlBuilderExtensions
{
    public static void PersistToMySql(this IElmahBuilder builder, Action<MySqlErrorLogOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.PersistTo(provider => new MySqlErrorLog(provider.GetRequiredService<IOptions<MySqlErrorLogOptions>>()));
    }
}
