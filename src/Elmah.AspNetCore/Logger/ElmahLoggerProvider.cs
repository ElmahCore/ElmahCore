using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Elmah.AspNetCore.Logger;

internal sealed class ElmahLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private IExternalScopeProvider? _scopeProvider;

    public ElmahLoggerProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string name)
    {
        return new ElmahLogger(name, null, _scopeProvider, _httpContextAccessor);
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }
}