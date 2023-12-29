using System;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Elmah.AspNetCore.Assertions;

[Serializable]
internal class DataBindingException : Exception
{
    public DataBindingException()
    {
    }

    public DataBindingException(string message) :
        // ReSharper disable once IntroduceOptionalParameters.Global
        this(message, null)
    {
    }

    public DataBindingException(string message, Exception? inner) :
        base(message, inner)
    {
    }
}