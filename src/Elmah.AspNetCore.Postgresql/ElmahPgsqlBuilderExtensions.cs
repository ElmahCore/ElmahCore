using System;
using Elmah.AspNetCore.Postgresql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Elmah;

public static class ElmahPgsqlBuilderExtensions
{
    public static void PersistToPgsql(this IElmahBuilder builder, Action<PgsqlErrorLogOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.PersistTo(provider => new PgsqlErrorLog(provider.GetRequiredService<IOptions<PgsqlErrorLogOptions>>()));
    }
}
