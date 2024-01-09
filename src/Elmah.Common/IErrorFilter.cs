namespace Elmah;

public interface IErrorFilter
{
    void OnErrorModuleFiltering(object sender, ExceptionFilterEventArgs args);
}