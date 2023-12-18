using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ElmahCore.Mvc.Tests
{
    public class ErrorLogMiddlewareTests
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly ErrorLog _errorLog = new MemoryErrorLog();
        private readonly IOptions<ElmahOptions> _options;
        private readonly IElmahExceptionLogger _elmahLogger;

        public ErrorLogMiddlewareTests()
        {
            _requestDelegate = httpContext => Task.CompletedTask;
            _options = Substitute.For<IOptions<ElmahOptions>>();
            _elmahLogger = new ElmahExceptionLogger(_errorLog, _options, NullLogger<ElmahExceptionLogger>.Instance);
        }

        [Fact]
        public void RiseErrorOkWhenMiddlewareInitialized()
        {
            var _ = new ErrorLogMiddleware(_requestDelegate, _elmahLogger, _options);
            var httpContext = new DefaultHttpContext();
            Func<Task> act = async () => await ElmahExtensions.RaiseError(httpContext, new Exception());
            act.Should().NotThrowAsync();
        }
    }
}