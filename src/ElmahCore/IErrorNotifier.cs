namespace ElmahCore;

public interface IErrorNotifier
{
    string Name { get; }
    void Notify(Error error);
}
public interface IErrorNotifierWithId : IErrorNotifier
{
    void Notify(string id, Error error);
}