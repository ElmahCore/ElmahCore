namespace ElmahCore
{
    public interface IErrorNotifier
    {
        void Notify(Error error);
        string Name { get; }
    }
}