using System.Diagnostics;

namespace ElmahCore.DemoCore6;

public class MyNotifier : IErrorNotifier
{
    public void Notify(Error error)
    {
        Debug.WriteLine(error.Message);
    }

    public string Name => "my";
}