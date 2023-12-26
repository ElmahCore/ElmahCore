using System;
using ElmahCore.Mvc.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElmahCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddElmahCoreServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.AddSingleton<IElmahExceptionLogger, ElmahExceptionLogger>();
        services.AddSingleton<ILoggerProvider, ElmahLoggerProvider>();
        services.AddSingleton<IErrorFactory, ErrorFactory>();
        
#if USE_GLOBAL_ERROR_HANDLING
        services.AddExceptionHandler<ElmahExceptionHandler>();
#endif

        return services;
    }
}
