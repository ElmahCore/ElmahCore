
using System;
using System.Runtime.Serialization;

namespace ElmahCore
{

    /// <summary>
    /// The exception that is thrown when a non-fatal error occurs. 
    /// This exception also serves as the base for all exceptions thrown by
    /// this library.
    /// </summary>

    [ Serializable ]
    internal class ApplicationException : Exception
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationException"/> class.
        /// </summary>
        
        public ApplicationException() {}
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationException"/> class 
        /// with a specified error message.
        /// </summary>

        public ApplicationException(string message) : 
            base(message) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationException"/> 
        /// class with a specified error message and a reference to the 
        /// inner exception that is the cause of this exception.
        /// </summary>

        public ApplicationException(string message, Exception innerException) : 
            base(message, innerException) {}
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationException"/> class 
        /// with serialized data.
        /// </summary>

        protected ApplicationException(SerializationInfo info, StreamingContext context) : 
            base(info, context) {}
    }
}

