using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elmah.AspNetCore.Handlers;

internal static partial class Endpoints
{
    public static IEndpointConventionBuilder MapTest(this IEndpointRouteBuilder builder, string prefix = "")
    {
        var handler = RequestDelegateFactory.Create((HttpContext _) => { throw new TestException(); });

        var pipeline = builder.CreateApplicationBuilder();
        pipeline.Run(handler.RequestDelegate);
        return builder.MapMethods($"{prefix}/test", new[] { HttpMethods.Get, HttpMethods.Post }, pipeline.Build());
    }
}
