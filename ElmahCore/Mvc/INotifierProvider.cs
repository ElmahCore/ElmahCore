using System.Collections.Generic;

namespace ElmahCore
{
    public interface INotifierProvider
    {
        IEnumerable<string> Notifiers { get; }

    }
}