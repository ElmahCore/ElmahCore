using System;
using System.Runtime.Serialization;

namespace ElmahCore.Mvc;

#region Imports

#endregion

/// <summary>
///     The exception that is thrown when to test the error logging
///     subsystem. This exception is used for testing purposes only and
///     should not be used for any other purpose.
/// </summary>
[Serializable]
internal sealed class TestException : System.ApplicationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TestException" /> class.
    /// </summary>
    public TestException() :
        this("This is a test exception that can be safely ignored.")
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TestException" /> class
    ///     with a specified error message.
    /// </summary>
    public TestException(string message) :
        base(message)
    {
    }

    /// <summary>
    ///     ializes a new instance of the <see cref="TestException" />
    ///     class with a specified error message and a reference to the
    ///     inner exception that is the cause of this exception.
    /// </summary>
    public TestException(string message, Exception innerException) :
        base(message, innerException)
    {
    }

#if !NET8_0_OR_GREATER
    /// <summary>
    ///     Initializes a new instance of the <see cref="TestException" /> class
    ///     with serialized data.
    /// </summary>
    private TestException(SerializationInfo info, StreamingContext context) :
        base(info, context)
    {
    }
#endif
}