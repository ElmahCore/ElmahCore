using System.Diagnostics.CodeAnalysis;
using Elmah.AspNetCore;
using Elmah.AspNetCore.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Elmah;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapElmah(this IEndpointRouteBuilder builder) => builder.MapElmah("/elmah");

    public static IEndpointConventionBuilder MapElmah(this IEndpointRouteBuilder builder, [StringSyntax("Route")]string prefix)
    {
        // HACK: we're using the options instance as global configuration. It might make more sense to create our
        // own object to store configuration context that is shared.
        var options = builder.ServiceProvider.GetRequiredService<IOptions<ElmahOptions>>().Value;
        options.Path = prefix;

#if NET7_0_OR_GREATER
        var group = builder.MapGroup(prefix);
        group.MapRoot();
        group.MapApiError();
        group.MapApiErrors();
        group.MapApiNewErrors();
        group.MapRss();
        group.MapDigestRss();
        group.MapMsdn();
        group.MapMsdnStatus();
        group.MapJson();
        group.MapXml();
        group.MapDownload();
        group.MapTest();
        group.MapResources();
        return group;
#else
        var routes = new[]
        {
            builder.MapRoot(prefix),
            builder.MapApiError(prefix),
            builder.MapApiErrors(prefix),
            builder.MapApiNewErrors(prefix),
            builder.MapRss(prefix),
            builder.MapDigestRss(prefix),
            builder.MapMsdn(prefix),
            builder.MapMsdnStatus(prefix),
            builder.MapXml(prefix),
            builder.MapJson(prefix),
            builder.MapDownload(prefix),
            builder.MapTest(prefix),
            builder.MapResources(prefix)
        };

        return new ElmahEndpointCollection(routes);
#endif
    }
}
