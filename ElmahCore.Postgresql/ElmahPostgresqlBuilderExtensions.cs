using System;
using ElmahCore.Postgresql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ElmahCore;

public static class ElmahPostgresqlBuilderExtensions
{
    public static void PersistToPostgresql(this IElmahBuilder builder, Action<PgsqlErrorLogOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.PersistTo(provider => new PgsqlErrorLog(provider.GetRequiredService<IOptions<PgsqlErrorLogOptions>>()));
    }
}
