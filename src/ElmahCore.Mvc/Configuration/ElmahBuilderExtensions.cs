using System;
using ElmahCore.Mvc;
using ElmahCore.Mvc.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ElmahCore;

public static class ElmahBuilderExtensions
{
    public static void UseElmahExceptionPage(this IElmahBuilder builder)
    {
        builder.Configure(o => o.ShowElmahErrorPage = true);
    }

    public static void Configure(this IElmahBuilder builder, Action<ElmahOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
    }

    public static void PersistToMemory(this IElmahBuilder builder)
    {
        builder.PersistToMemory(o => { });
    }

    public static void PersistToMemory(this IElmahBuilder builder, Action<MemoryErrorLogOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.PersistTo(provider => new MemoryErrorLog(provider.GetRequiredService<IOptions<MemoryErrorLogOptions>>()));
    }

    public static void PersistToFile(this IElmahBuilder builder, string logPath)
    {
        builder.PersistToFile(o => o.LogPath = logPath); 
    }

    public static void PersistToFile(this IElmahBuilder builder, Action<XmlFileErrorLogOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.PersistTo<XmlFileErrorLog>();
    }

    public static void SetLogLevel(this IElmahBuilder builder, LogLevel level)
    {
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddFilter<ElmahLoggerProvider>(l => l >= level); 
        });
    }

    public static void AddSqlDiagnostics(this IElmahBuilder builder)
    {
        builder.Services.AddSingleton<ElmahDiagnosticObserver>();
    }
}