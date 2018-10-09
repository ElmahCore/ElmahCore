using System;
using ElmahCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace ElmahCore
{
    public static class ElmahExtensions
    {
        public static void RiseError(this HttpContext ctx, Exception ex)
        {
            var middleware = ctx.RequestServices.GetService<ErrorLogMiddleware>();
            middleware?.LogException(ex,ctx);
        }
    }
}