using System;
using ElmahCore.Mvc.Logger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElmahCore.Mvc
{
    public static class BuilderHelper
    {
        public static IApplicationBuilder UseElmah(this IApplicationBuilder app)
        {
            //app.UseMiddleware<ErrorLogMiddleware>();
            app.Use(async (ctx, next) =>
            {
                var middleware = ctx.RequestServices.GetService<ErrorLogMiddleware>();
                await middleware.Invoke(ctx, next);
            });
            return app;
        }

        public static IServiceCollection AddElmah(this IServiceCollection services)
        {
            return AddElmah<MemoryErrorLog>(services);
        }

        public static IServiceCollection AddElmah<T>(this IServiceCollection services) where T : ErrorLog
        {
            services.AddSingleton<ErrorLogMiddleware>();
            services.AddHttpContextAccessor();
            services.AddSingleton<ILoggerProvider>(provider => new ElmahLoggerProvider(provider.GetService<IHttpContextAccessor>()));

            return services.AddSingleton<ErrorLog, T>();
        }

        public static IServiceCollection AddElmah(this IServiceCollection services, Action<ElmahOptions> setupAction)
        {
            return AddElmah<MemoryErrorLog>(services, setupAction);
        }

        public static IServiceCollection AddElmah<T>(this IServiceCollection services, Action<ElmahOptions> setupAction)
            where T : ErrorLog
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            var builder = services.AddElmah<T>();
            builder.Configure(setupAction);
            return builder;
        }

    }
}