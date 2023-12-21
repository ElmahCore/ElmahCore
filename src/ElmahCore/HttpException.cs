using System;
using System.Net;

namespace ElmahCore;

[Serializable]
internal class HttpException : Exception
{
    public HttpException(int statusCode) : base(((HttpStatusCode) statusCode).ToString())
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}