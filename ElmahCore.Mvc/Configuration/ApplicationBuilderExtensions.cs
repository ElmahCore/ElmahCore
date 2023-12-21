using System.Diagnostics;
using ElmahCore.Mvc;
using Microsoft.AspNetCore.Builder;

namespace ElmahCore
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseElmah(this IApplicationBuilder app)
        {
            DiagnosticListener.AllListeners.Subscribe(new ElmahDiagnosticObserver(app.ApplicationServices));

            app.UseMiddleware<ErrorLogMiddleware>();
            return app;
        }
    }
}