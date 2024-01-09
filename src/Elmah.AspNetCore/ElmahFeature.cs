using System;

namespace Elmah.AspNetCore;

internal class ElmahFeature : IElmahFeature
{
    public ElmahFeature(Guid id, string location)
    {
        Id = id;
        Location = location;
    }

    public Guid Id { get; }

    public string Location { get; }
}
