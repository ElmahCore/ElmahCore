using System;
using System.Threading.Tasks;
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
            Func<Task> act = async () => await ElmahExtensions.RiseError(httpContext, new Exception());
            act.Should().ThrowAsync<MiddlewareNotInitializedException>();
        }
    }
}