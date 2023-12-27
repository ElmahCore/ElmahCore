using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace ElmahCore;

/// <summary>
///     Represents a logical application error (as opposed to the actual
///     exception it may be representing).
/// </summary>
[Serializable]
public sealed class Error : ICloneable
{
    private string? _applicationName;
    private NameValueCollection? _cookies;
    private string? _detail;
    private NameValueCollection? _form;
    private string? _hostName;
    private string? _message;
    private NameValueCollection? _queryString;
    private NameValueCollection? _serverVariables;
    private string? _source;
    private string? _typeName;
    private string? _user;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Error" /> class.
    /// </summary>
    public Error()
    {
    }

    public Guid Id { get; init; } = Guid.NewGuid();

    public ElmahLogMessageEntry[] MessageLog { get; internal set; } = Array.Empty<ElmahLogMessageEntry>();

    public ElmahLogSqlEntry[] SqlLog { get; internal set; } = Array.Empty<ElmahLogSqlEntry>();

    public ElmahLogParamEntry[] Params { get; internal set; } = Array.Empty<ElmahLogParamEntry>();

    /// <summary>
    ///     Gets the <see cref="Exception" /> instance used to initialize this
    ///     instance.
    /// </summary>
    /// <remarks>
    ///     This is a run-time property only that is not written or read
    ///     during XML serialization via <see cref="ErrorXml.Decode" /> and
    ///     <see cref="ErrorXml.Encode(Error,XmlWriter)" />.
    /// </remarks>
    public Exception? Exception { get; init; }

    /// <summary>
    ///     Gets or sets the name of application in which this error occurred.
    /// </summary>
    [AllowNull]
    public string ApplicationName
    {
        get => _applicationName ?? string.Empty;
        set => _applicationName = value;
    }

    /// <summary>
    ///     Gets or sets name of host machine where this error occurred.
    /// </summary>
    [AllowNull]
    public string HostName
    {
        get => _hostName ?? Environment.GetEnvironmentVariable("COMPUTERNAME") ??
            Environment.GetEnvironmentVariable("HOSTNAME") ?? string.Empty;
        set => _hostName = value;
    }

    /// <summary>
    ///     Gets or sets the type, class or category of the error.
    /// </summary>
    [AllowNull]
    public string Type
    {
        get => _typeName ?? string.Empty;
        set => _typeName = value;
    }

    /// <summary>
    ///     Gets or sets the Request Body
    /// </summary>

    public string? Body => _form is null ? null : _form["$request-body"] ?? string.Empty;

    /// <summary>
    ///     Gets or sets the source that is the cause of the error.
    /// </summary>
    [AllowNull]
    public string Source
    {
        get => _source ?? string.Empty;
        set => _source = value;
    }

    /// <summary>
    ///     Gets or sets a brief text describing the error.
    /// </summary>
    [AllowNull]
    public string Message
    {
        get => _message ?? string.Empty;
        set => _message = value;
    }

    /// <summary>
    ///     Gets or sets a detailed text describing the error, such as a
    ///     stack trace.
    /// </summary>
    [AllowNull]
    public string Detail
    {
        get => _detail ?? string.Empty;
        set => _detail = value;
    }

    /// <summary>
    ///     Gets or sets the user logged into the application at the time
    ///     of the error.
    /// </summary>
    [AllowNull]
    public string User
    {
        get => _user ?? Environment.GetEnvironmentVariable("USERDOMAIN") ??
            Environment.GetEnvironmentVariable("USERNAME") ?? string.Empty;

        set => _user = value;
    }

    /// <summary>
    ///     Gets or sets the date and time (in local time) at which the
    ///     error occurred.
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    ///     Gets or sets the HTTP status code of the output returned to the
    ///     client for the error.
    /// </summary>
    /// <remarks>
    ///     For cases where this value cannot always be reliably determined,
    ///     the value may be reported as zero.
    /// </remarks>
    public int StatusCode { get; set; }

    /// <summary>
    ///     Gets a collection representing the Web server variables
    ///     captured as part of diagnostic data for the error.
    /// </summary>
    public NameValueCollection ServerVariables
    {
        get => _serverVariables ??= new NameValueCollection();
        internal set => _serverVariables = value;
    }

    /// <summary>
    ///     Gets a collection representing the Web query string variables
    ///     captured as part of diagnostic data for the error.
    /// </summary>
    [AllowNull]
    public NameValueCollection QueryString
    {
        get => _queryString ??= new NameValueCollection();
        internal set => _queryString = value;
    }

    /// <summary>
    ///     Gets a collection representing the form variables captured as
    ///     part of diagnostic data for the error.
    /// </summary>
    [AllowNull]
    public NameValueCollection Form
    {
        get => _form ??= new NameValueCollection();
        internal set => _form = value;
    }

    /// <summary>
    ///     Gets a collection representing the client cookies
    ///     captured as part of diagnostic data for the error.
    /// </summary>
    [AllowNull]
    public NameValueCollection Cookies
    {
        get => _cookies ??= new NameValueCollection();
        internal set => _cookies = value;
    }

    /// <summary>
    ///     Creates a new object that is a copy of the current instance.
    /// </summary>
    object ICloneable.Clone()
    {
        //
        // Make a base shallow copy of all the members.
        //
        var copy = (Error) MemberwiseClone();

        //
        // Now make a deep copy of items that are mutable.
        //
        copy._serverVariables = CopyCollection(_serverVariables);
        copy._queryString = CopyCollection(_queryString);
        copy._form = CopyCollection(_form);
        copy._cookies = CopyCollection(_cookies);

        return copy;
    }

    /// <summary>
    ///     Returns the value of the <see cref="Message" /> property.
    /// </summary>
    public override string ToString() => Message;

    public Error Clone() => (Error)((ICloneable)this).Clone();

    private static NameValueCollection? CopyCollection(NameValueCollection? collection)
    {
        if (collection is null || collection.Count == 0)
        {
            return null;
        }

        return new NameValueCollection(collection);
    }
}