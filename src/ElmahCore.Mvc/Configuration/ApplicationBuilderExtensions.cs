using System.Diagnostics;
using ElmahCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ElmahCore;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseElmah(this IApplicationBuilder app)
    {
        var observer = app.ApplicationServices.GetService<ElmahDiagnosticObserver>();
        if (observer is not null)
        {
            DiagnosticListener.AllListeners.Subscribe(observer);
        }

        app.UseMiddleware<ErrorLogMiddleware>();
        return app;
    }
}