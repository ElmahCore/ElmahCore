using System.Diagnostics;

namespace Elmah.AspNetCore.DemoCore6;

public class MyNotifier : IErrorNotifier
{
    public Task NotifyAsync(Error error)
    {
        Debug.WriteLine(error.Message);
        return Task.CompletedTask;
    }

    public string Name => "my";
}