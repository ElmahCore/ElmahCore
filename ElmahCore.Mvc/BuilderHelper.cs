using System;
using System.Diagnostics;
using System.Linq;
using ElmahCore.Mvc.Logger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ElmahCore.Mvc
{
    public static class BuilderHelper
    {
        public static IApplicationBuilder UseElmahExceptionPage(this IApplicationBuilder app)
        {
            ErrorLogMiddleware.ShowDebugPage = true;
            return app;
        }

        public static IApplicationBuilder UseElmah(this IApplicationBuilder app)
        {
            DiagnosticListener.AllListeners.Subscribe(new ElmahDiagnosticObserver(app.ApplicationServices));

            app.UseMiddleware<ErrorLogMiddleware>();

            return app;
        }

        public static IServiceCollection AddElmah(this IServiceCollection services)
        {
            return AddElmah<MemoryErrorLog>(services);
        }

        public static IServiceCollection AddElmah<T>(this IServiceCollection services) where T : ErrorLog
        {
            return services.AddElmah<T>(o => { });
        }

        public static IServiceCollection SetElmahLogLevel(this IServiceCollection services, LogLevel level)
        {
            services.AddLogging(builder => { builder.AddFilter<ElmahLoggerProvider>(l => l >= level); });
            return services;
        }

        public static IServiceCollection AddElmah(this IServiceCollection services, Action<ElmahOptions> setupAction)
        {
            return AddElmah<MemoryErrorLog>(services, setupAction);
        }

        public static IServiceCollection AddElmah<T>(this IServiceCollection services, Action<ElmahOptions> setupAction)
            where T : ErrorLog
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

            services.AddHttpContextAccessor();
            services.AddSingleton<IElmahExceptionLogger, ElmahExceptionLogger>();
            services.AddSingleton<ILoggerProvider>(provider =>
                new ElmahLoggerProvider(provider.GetRequiredService<IHttpContextAccessor>()));

            services.AddSingleton<T>();
            services.AddSingleton<ErrorLog, T>(provider =>
            {
                var log = provider.GetRequiredService<T>();
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
            });

            services.Configure(setupAction);
            services.PostConfigure<ElmahOptions>(o =>
            {
                string elmahRoot = o.Path;
                if (elmahRoot[0] == '~')
                {
                    elmahRoot = elmahRoot[1..];
                }

                if (elmahRoot[0] != '/')
                {
                    elmahRoot = "/" + elmahRoot;
                }

                o.Path = elmahRoot.TrimEnd('/');
            });

            return services;
        }
    }
}