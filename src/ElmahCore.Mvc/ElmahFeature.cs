using System;

namespace ElmahCore.Mvc;

public interface IElmahFeature
{
    public Guid Id { get; }

    public string Location { get; }
}

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
