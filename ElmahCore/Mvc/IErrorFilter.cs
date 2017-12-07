namespace ElmahCore
{
    public interface IErrorFilter
    {
        void OnErrorModuleFiltering(object sender, ExceptionFilterEventArgs args);
    }
}