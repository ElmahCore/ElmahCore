using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
namespace Elmah.AspNetCore.Logger;

internal sealed class ElmahLogger : ILogger
{
    private readonly IHttpContextAccessor _accessor;
    private Func<string, LogLevel, bool> _filter;

    internal ElmahLogger(string name, Func<string, LogLevel, bool>? filter, IExternalScopeProvider? scopeProvider,
        IHttpContextAccessor accessor)
    {
        _accessor = accessor;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _filter = filter ?? ((category, logLevel) => true);
        ScopeProvider = scopeProvider;
    }

    public Func<string, LogLevel, bool> Filter
    {
        get => _filter;
        set => _filter = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Name { get; }

    internal IExternalScopeProvider? ScopeProvider { get; set; }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        if (!IsEnabled(logLevel))
        {
            return;
        }

        var feature = _accessor.HttpContext!.Features.Get<IElmahLogFeature>()!;
        var entry = new ElmahLoggerMessage<TState>
        {
            TimeStamp = DateTime.Now,
            State = state,
            Exception = exception,
            Formatter = formatter,
            Level = logLevel
        };

        feature.AddMessage(entry);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None &&
               Filter(Name, logLevel) &&
               _accessor.HttpContext?.Features.Get<IElmahLogFeature>() != null;
    }

#if NET6_0
    public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;
#else
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => ScopeProvider?.Push(state);
#endif
}
