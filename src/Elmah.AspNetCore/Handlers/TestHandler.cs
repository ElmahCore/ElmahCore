using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elmah.AspNetCore.Handlers;

internal static partial class Endpoints
{
    public static IEndpointConventionBuilder MapTest(this IEndpointRouteBuilder builder, string prefix = "")
    {
        return builder.MapMethods($"{prefix}/test", new[] { HttpMethods.Get, HttpMethods.Post }, (HttpContext _) => throw new TestException());
    }
}
