using System.Threading.Tasks;

namespace Elmah;

public interface IErrorNotifier
{
    string Name { get; }

    Task NotifyAsync(Error error);
}