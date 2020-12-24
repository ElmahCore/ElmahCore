using System;
using System.Diagnostics;
using System.Reflection;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace ElmahCore.Assertions
{
    internal sealed class AssertionHelperContext
    {
        private Exception _baseException;
        private int _httpStatusCode;
        private bool _statusCodeInitialized;

        public AssertionHelperContext(Exception e, object context) :
            this(null, e, context)
        { }

        public AssertionHelperContext(object source, Exception e, object context)
        {
            Debug.Assert(e != null);

            FilterSource = source ?? this;
            Exception = e;
            Context = context;
        }

        public object FilterSource { get; }

        public Type FilterSourceType => FilterSource.GetType();

        public AssemblyName FilterSourceAssemblyName => FilterSourceType.Assembly.GetName();

        public Exception Exception { get; }

        public Exception BaseException => _baseException ??= Exception.GetBaseException();

        public bool HasHttpStatusCode => HttpStatusCode != 0;

        public int HttpStatusCode
        {
            get
            {
                if (_statusCodeInitialized) return _httpStatusCode;

                _statusCodeInitialized = true;

                if (Exception is HttpException exception)
                    _httpStatusCode = exception.StatusCode;

                return _httpStatusCode;
            }
        }

        public object Context { get; }
    }
}