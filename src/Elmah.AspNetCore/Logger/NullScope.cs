#if NET6_0
using System;

namespace Elmah.AspNetCore.Logger;

internal sealed class NullScope : IDisposable
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