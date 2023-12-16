using System;
using System.Diagnostics;
using ElmahCore.Mvc;
using ElmahCore.Mvc.Logger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ElmahCore
{
    public static class ServiceCollectionExtensions
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

        /// <summary>
        /// Registers Elmah services and in-memory error logging.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns></returns>
        public static IServiceCollection AddElmah(this IServiceCollection services)
        {
            return services.AddElmah(builder => builder.PersistInMemory());
        }

        public static IServiceCollection AddElmah(this IServiceCollection services, Action<ElmahBuilder> configureElmah)
        {
            services.AddElmahCoreServices();

            var elmah = new ElmahBuilder(services);
            configureElmah(elmah);
            
            return services;
        }

        public static void Configure(this IElmahBuilder builder, Action<ElmahOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
        }

        public static void PersistInMemory(this IElmahBuilder builder)
        {
            builder.PersistInMemory(o => { });
        }

        public static void PersistInMemory(this IElmahBuilder builder, Action<MemoryErrorLogOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.PersistTo(provider => new MemoryErrorLog(provider.GetRequiredService<IOptions<MemoryErrorLogOptions>>()));
        }

        public static void PersistToFile(this IElmahBuilder builder, string logPath)
        {
            builder.Services.Configure<XmlFileErrorLogOptions>(o => o.LogPath = logPath); 
            builder.PersistTo<XmlFileErrorLog>();
        }

        public static void PersistToFile(this IElmahBuilder builder, Action<XmlFileErrorLogOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.PersistTo<XmlFileErrorLog>();
        }

        public static void SetLogLevel(this IElmahBuilder builder, LogLevel level)
        {
            builder.Services.AddLogging(builder => { builder.AddFilter<ElmahLoggerProvider>(l => l >= level); });
        }

        public static IServiceCollection AddElmahCoreServices(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHttpContextAccessor();
            services.AddSingleton<IElmahExceptionLogger, ElmahExceptionLogger>();
            services.AddSingleton<ILoggerProvider>(provider =>
                new ElmahLoggerProvider(provider.GetRequiredService<IHttpContextAccessor>()));

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