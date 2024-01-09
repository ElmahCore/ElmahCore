using System;

namespace Elmah.AspNetCore;

public interface IElmahFeature
{
    public Guid Id { get; }

    public string Location { get; }
}
