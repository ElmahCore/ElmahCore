using System;

namespace ElmahCore.Mvc.Exceptions
{
    /// <inheritdoc />
    public class MiddlewareNotInitializedException : Exception
    {
        // ReSharper disable once UnusedMember.Global
        public MiddlewareNotInitializedException()
        {
        }

        public MiddlewareNotInitializedException(string message) : base(message)
        {
        }

        // ReSharper disable once UnusedMember.Global

        public MiddlewareNotInitializedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}