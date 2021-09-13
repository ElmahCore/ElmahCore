using System;

namespace ElmahCore.Mvc.Exceptions
{
    public class MiddlewareNotInitializedException : Exception
    {
        public MiddlewareNotInitializedException(string message) : base(message)
        {
        }

        public MiddlewareNotInitializedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}