#if NET6_0
using System;

namespace ElmahCore.Mvc.Logger;

public class NullScope : IDisposable
{
    private NullScope()
    {
    }

    public static NullScope Instance { get; } = new NullScope();

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
#endif