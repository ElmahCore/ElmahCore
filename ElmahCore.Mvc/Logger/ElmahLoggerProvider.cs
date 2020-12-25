using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ElmahCore.Mvc.Logger
{
    public class ElmahLoggerProvider: ILoggerProvider, ISupportExternalScope
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IExternalScopeProvider _scopeProvider;

        public ElmahLoggerProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string name)
        {
            return new ElmahLogger(name, (s, l) => true, _scopeProvider, _httpContextAccessor);
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }
    }
}
