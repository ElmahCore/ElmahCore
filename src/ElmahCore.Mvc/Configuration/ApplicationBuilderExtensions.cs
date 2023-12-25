using System;
using System.Diagnostics;
using ElmahCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ElmahCore;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseElmahMiddleware(this IApplicationBuilder app)
    {
        var observer = app.ApplicationServices.GetService<ElmahDiagnosticObserver>();
        if (observer is not null)
        {
            DiagnosticListener.AllListeners.Subscribe(observer);
        }

        app.UseMiddleware<ErrorLogMiddleware>();
        return app;
    }

    public static IHostBuilder UseElmah(this IHostBuilder host)
    {
        return host.UseElmah(null);
    }

    public static IHostBuilder UseElmah(this IHostBuilder host, Action<HostBuilderContext, ElmahBuilder>? configureElmah)
    {
        host.ConfigureServices((builderContext, services) =>
        {
            services.AddElmahCoreServices();

            var elmah = new ElmahBuilder(services);

            // Set as default because it is required - consumer can replace in configure delegate
            elmah.PersistToMemory();

            configureElmah?.Invoke(builderContext, elmah);
        });

        return host;
    }
}