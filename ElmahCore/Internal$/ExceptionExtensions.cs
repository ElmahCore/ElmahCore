using System;
using System.Diagnostics;

namespace ElmahCore
{
    internal static class ExceptionExtensions
    {
        private const string CallerInfoKey = "ElmahCallerInfo";

	    public static CallerInfo TryGetCallerInfo(this Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            var datum = exception.IsData() 
                      ? exception.Data[CallerInfoKey] 
                      : null;
            return datum as CallerInfo;
        }

	    static bool IsData(this Exception exception, bool writable = false)
        {
            Debug.Assert(exception != null);

            var data = exception.Data;

            // "The ExecutionEngineException, OutOfMemoryException, 
            //  StackOverflowException and ThreadAbortException classes 
            //  always return null for the value of the Data property."
            //
            // http://msdn.microsoft.com/en-us/library/system.exception.data(v=vs.80).aspx

                                // ReSharper disable ConditionIsAlwaysTrueOrFalse
            return data != null // ReSharper restore ConditionIsAlwaysTrueOrFalse
                && (!writable || !data.IsReadOnly);
        }
    }
}