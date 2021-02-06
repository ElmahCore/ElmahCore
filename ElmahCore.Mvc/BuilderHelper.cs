using System;
using System.Diagnostics;
using ElmahCore.Mvc.Logger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            app.UseStaticHttpContext();

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
            services.AddHttpContextAccessor();
            services.AddSingleton<ILoggerProvider>(provider =>
                new ElmahLoggerProvider(provider.GetService<IHttpContextAccessor>()));

            return services.AddSingleton<ErrorLog, T>();
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

            var builder = services.AddElmah<T>();
            builder.Configure(setupAction);
            return builder;
        }
    }

    internal static class InternalHttpContext
    {
        private static IHttpContextAccessor _contextAccessor;

        public static HttpContext Current => _contextAccessor.HttpContext;

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
    }

    internal static class StaticHttpContextExtensions
    {
        public static void AddHttpContextAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
        {
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            InternalHttpContext.Configure(httpContextAccessor);
            return app;
        }
    }
}