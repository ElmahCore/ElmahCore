using System;
using ElmahCore.Mvc;
using ElmahCore.Mvc.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElmahCore;

public static class ServiceCollectionExtensions
{
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

#if USE_GLOBAL_ERROR_HANDLING
            services.AddExceptionHandler<ElmahExceptionHandler>();
#endif

        return services;
    }
}
