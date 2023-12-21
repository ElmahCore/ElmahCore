namespace ElmahCore.Mvc;

public interface IElmahFeature
{
    public string Id { get; }

    public string Location { get; }
}

internal class ElmahFeature : IElmahFeature
{
    public ElmahFeature(string id, string location)
    {
        Id = id;
        Location = location;
    }

    public string Id { get; }

    public string Location { get; }
}
