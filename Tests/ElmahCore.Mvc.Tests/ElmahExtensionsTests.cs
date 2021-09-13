using System;
using ElmahCore.Mvc.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ElmahCore.Mvc.Tests
{
    public class ElmahExtensionsTests
    {
        [Fact]
        public void RiseErrorExceptionWhenMiddlewareNotInitialised()
        {
            var httpContext = new DefaultHttpContext();
            Action act = () => ElmahExtensions.RiseError(httpContext, new Exception());
            act.Should().Throw<MiddlewareNotInitializedException>();
        }

    }
}