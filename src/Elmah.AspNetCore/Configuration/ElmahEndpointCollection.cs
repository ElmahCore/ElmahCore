#if NET6_0
using System;
using Microsoft.AspNetCore.Builder;

namespace Elmah.AspNetCore;

internal class ElmahEndpointCollection : IEndpointConventionBuilder
{
    private readonly IEndpointConventionBuilder[] _routes;

    public ElmahEndpointCollection(IEndpointConventionBuilder[] routes)
    {
        _routes = routes;
    }

    public void Add(Action<EndpointBuilder> convention)
    {
        foreach (var route in _routes)
        {
            route.Add(convention);
        }
    }
}
#endif
