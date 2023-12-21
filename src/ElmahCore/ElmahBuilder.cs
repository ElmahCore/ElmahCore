using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ElmahCore;

public class ElmahBuilder : IElmahBuilder
{
    private readonly IServiceCollection _services;

    internal ElmahBuilder(IServiceCollection services)
    {
        _services = services;
    }

    IServiceCollection IElmahBuilder.Services => _services;

    public void PersistTo<T>() where T : ErrorLog
    {
        _services.AddSingleton<T>();
        _services.Replace(ServiceDescriptor.Singleton(provider =>
        {
            var log = provider.GetRequiredService<T>();
            return this.ConfigureErrorLog(provider, log);
        }));
    }

    public void PersistTo(ErrorLog log)
    {
        _services.Replace(ServiceDescriptor.Singleton(provider =>
        {
            return this.ConfigureErrorLog(provider, log);
        }));
    }

    public void PersistTo(Func<IServiceProvider, ErrorLog> factory)
    {
        _services.Replace(ServiceDescriptor.Singleton(provider =>
        {
            var log = factory(provider);
            return this.ConfigureErrorLog(provider, log);
        }));
    }

    private ErrorLog ConfigureErrorLog(IServiceProvider provider, ErrorLog log)
    {
        var options = provider.GetRequiredService<IOptions<ElmahOptions>>().Value;

        if (!string.IsNullOrWhiteSpace(options.ApplicationName))
        {
            log.ApplicationName = options.ApplicationName;
        }

        if (options.SourcePaths?.Any() ?? false)
        {
            log.SourcePaths = options.SourcePaths;
        }

        return log;
    }
}