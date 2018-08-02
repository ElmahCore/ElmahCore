using System;
using System.Diagnostics;
using System.Reflection;

namespace ElmahCore.Assertions
{
    internal sealed class AssertionHelperContext
    {
        private readonly object _source;
        private readonly Exception _exception;
        private readonly object _context;
        private Exception _baseException;
        private int _httpStatusCode;
        private bool _statusCodeInitialized;

        public AssertionHelperContext(Exception e, object context) :
            this(null, e, context)
        { }

        public AssertionHelperContext(object source, Exception e, object context)
        {
            Debug.Assert(e != null);

            _source = source == null ? this : source;
            _exception = e;
            _context = context;
        }

        public object FilterSource
        {
            get { return _source; }
        }

        public Type FilterSourceType
        {
            get { return _source.GetType(); }
        }

        public AssemblyName FilterSourceAssemblyName
        {
            get { return FilterSourceType.Assembly.GetName(); }
        }

        public Exception Exception
        {
            get { return _exception; }
        }

        public Exception BaseException
        {
            get
            {
                if (_baseException == null)
                    _baseException = Exception.GetBaseException();

                return _baseException;
            }
        }

        public bool HasHttpStatusCode
        {
            get { return HttpStatusCode != 0; }
        }

        public int HttpStatusCode
        {
            get
            {
                if (!_statusCodeInitialized)
                {
                    _statusCodeInitialized = true;

                    var exception = Exception as HttpException;

                    if (exception != null)
                        _httpStatusCode = exception.StatusCode;
                }

                return _httpStatusCode;
            }
        }

        public object Context
        {
            get { return _context; }
        }
    }
}