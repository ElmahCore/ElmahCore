namespace Elmah;

public sealed class ErrorLogSearchFilter : IErrorLogFilter
{
    public ErrorLogSearchFilter(string searchText)
    {
        this.SearchText = searchText;
    }

    public string SearchText { get; }

    public bool IsMatch(ErrorLogEntry entry) => ErrorLogFilterHelper.DoSearch(entry, this.SearchText);
}
