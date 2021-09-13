using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ElmahCore.Mvc.Tests
{
    public class ErrorLogMiddlewareTests
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly ErrorLog _errorLog;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptions<ElmahOptions> _options;

        public ErrorLogMiddlewareTests()
        {
            _requestDelegate = httpContext => Task.CompletedTask;
            _options = Substitute.For<IOptions<ElmahOptions>>();
            _loggerFactory = Substitute.For<ILoggerFactory>();
            _errorLog = new MemoryErrorLog();
        }

        [Fact]
        public void WhenInitMiddlewareSetsStaticExtension()
        {
            var middleware = new ErrorLogMiddleware(_requestDelegate, _errorLog, _loggerFactory, _options);
            ElmahExtensions.LogMiddleware.Should().NotBeNull();
        }

        [Fact]
        public void RiseErrorOkWhenMiddlewareInitialized()
        {
            var middleware = new ErrorLogMiddleware(_requestDelegate, _errorLog, _loggerFactory, _options);
            var httpContext = new DefaultHttpContext();
            Func<Task> act = async () => await ElmahExtensions.RiseError(httpContext, new Exception());
            act.Should().NotThrowAsync();
        }
    }
}