namespace ElmahCore
{
    public interface IErrorNotifier
    {
        string Name { get; }
        void Notify(Error error);
    }
}