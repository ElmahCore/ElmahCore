using System.Diagnostics;
using ElmahCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ElmahCore;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseElmah(this IApplicationBuilder app)
    {
        DiagnosticListener.AllListeners.Subscribe(app.ApplicationServices.GetRequiredService<ElmahDiagnosticObserver>());

        app.UseMiddleware<ErrorLogMiddleware>();
        return app;
    }
}