using System;
using ElmahCore.Mvc.Logger;
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
        return services.AddElmah(builder => builder.PersistToMemory());
    }

    public static IServiceCollection AddElmah(this IServiceCollection services, Action<ElmahBuilder> configureElmah)
    {
        services.AddElmahCoreServices();

        var elmah = new ElmahBuilder(services);

        // Set as default because it is required - consumer can replace in configure delegate
        elmah.PersistToMemory();

        configureElmah(elmah);

        return services;
    }

    public static IServiceCollection AddElmahCoreServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.AddSingleton<IElmahExceptionLogger, ElmahExceptionLogger>();
        services.AddSingleton<ILoggerProvider, ElmahLoggerProvider>();
        
#if USE_GLOBAL_ERROR_HANDLING
        services.AddExceptionHandler<ElmahExceptionHandler>();
#endif

        return services;
    }
}
