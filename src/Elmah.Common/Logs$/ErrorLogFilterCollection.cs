using System.Collections.ObjectModel;

namespace Elmah;

public sealed class ErrorLogFilterCollection : Collection<IErrorLogFilter>, IErrorLogFilter
{
    public static readonly ErrorLogFilterCollection Empty = new();

    public bool IsMatch(ErrorLogEntry entry)
    {
        foreach (var filter in this)
        {
            if (!filter.IsMatch(entry))
            {
                return false;
            }
        }

        return true;
    }
}
