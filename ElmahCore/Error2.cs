using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.Json.Serialization;
using System.Threading;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

[assembly: InternalsVisibleTo("ElmahCore.Mvc")]


namespace ElmahCore
{
	/// <summary>
	/// Represents a logical application error (as opposed to the actual 
	/// exception it may be representing).
	/// </summary>

	[ Serializable ]
    public sealed class Error : ICloneable
    {
		private string _applicationName;
        private string _hostName;
        private string _typeName;
        private string _source;
        private string _message;
        private string _detail;
        private string _user;
		private string _webHostHtmlMessage;
        private NameValueCollection _serverVariables;
        private NameValueCollection _queryString;
        private NameValueCollection _form;
        private NameValueCollection _cookies;
        private NameValueCollection _header;
        private NameValueCollection _connection;
        private NameValueCollection _userData;
        private NameValueCollection _session;
        private NameValueCollection _items;
        public string Severity { get;  set; } 

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>

        public Error() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class
        /// from a given <see cref="Exception"/> instance and 
        /// <see cref="HttpContext"/> instance representing the HTTP 
        /// context during the exception.
        /// </summary>

        public Error(Exception e, HttpContext context = null)
        {
            var baseException = e?.GetBaseException();
            _message = baseException?.Message;
            _typeName = baseException?.GetType().FullName;

            //
            // If this is an HTTP exception, then get the status code
            // and detailed HTML message provided by the host.
            //
            if (baseException is HttpException httpExc)
            {
                StatusCode = httpExc.StatusCode;
                baseException = baseException.InnerException;
                if (baseException == null)
                {
                    _typeName = "HTTP";
                }
            }

            Exception = baseException;

            //
            // Load the basic information.
            //

            try
            {
                _hostName = Environment.MachineName;
            }
            catch (SecurityException)
            {
                // A SecurityException may occur in certain, possibly 
                // user-modified, Medium trust environments.
                _hostName = string.Empty;
            }

            _source = baseException?.Source;
            _detail = e?.ToString();
            _user = context.User?.Identity?.Name ?? string.Empty;
            Time = DateTime.Now;




            //
            // If the HTTP context is available, then capture the
            // collections that represent the state request as well as
            // the user.
            //

            if (context != null)
            {
                var webUser = context.User;
                if (webUser != null 
                    && (webUser.Identity.Name ?? string.Empty).Length > 0)
                {
                    _user = webUser.Identity.Name;
                }

                var request = context.Request;

                //Load Server Variables
                _serverVariables = GetServerValiables(context);
                _serverVariables.Add("HttpStatusCode", StatusCode.ToString());

                if (StatusCode == 0) Severity = "Error";
                else if (StatusCode < 200) Severity = "Info";
                else if (StatusCode < 400) Severity = "Success";
                else if (StatusCode < 500) Severity = "Warning";
                else Severity = "Error";

                
                Url = context?.Request?.Path.Value;
                Method = context?.Request?.Method;
                Client = context?.Connection?.RemoteIpAddress?.ToString();
                _queryString = CopyCollection(QueryHelpers.ParseQuery(request.QueryString.Value));
                _header = CopyCollection(request.Headers);

                _connection = new NameValueCollection();  LoadVariables(_connection, () => context.Connection,"");
                _userData = new NameValueCollection();  LoadVariables(_userData, () => context.User,"");
                _session = new NameValueCollection();  
    
                var ss = context.RequestServices?.GetService(typeof(ISession));
                if (ss != null)
                    LoadVariables(_session, () => context.Session,"");

                _items = new NameValueCollection();  LoadVariables(_items, () => context.Items,"");
                
                _form = CopyCollection(request.HasFormContentType ? request.Form : null);
                _cookies = CopyCollection(request.Cookies);
            }

            var callerInfo = e?.TryGetCallerInfo() ?? CallerInfo.Empty;
            if (!callerInfo.IsEmpty)
            {
                _detail = "# caller: " + callerInfo
                        + Environment.NewLine
                        + _detail;
            }
        }

        private NameValueCollection GetServerValiables(HttpContext context)
        {
            var serverVariables = new NameValueCollection();
            LoadVariables(serverVariables, () => context.Features, "");
            return serverVariables;
        }



        private void LoadVariables(NameValueCollection serverVariables, Func<object> getObject, string prefix)
        {
            object obj;
            try
            {
                obj = getObject();
                if (obj == null) return;
            }
            catch {return;}
            var props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                object value = null;
                try
                {
                    value = prop.GetValue(obj);
                }
	            catch
	            {
		            // ignored
	            }

	            bool isProcessed = false;
                if (value is IEnumerable en && !(en is string))
                {
	                if (value is IDictionary<object, object> dic)
                    {
                        if(dic.Keys.Count == 0) { continue; }
                    }
	                foreach (var item in en)
                    {
                        try
                        {
                            var keyProp = item.GetType().GetProperty("Key");
                            var valueProp = item.GetType().GetProperty("Value");

                            if (keyProp != null && valueProp != null && !prop.Name.StartsWith("RequestHeaders",StringComparison.InvariantCultureIgnoreCase))
                            {
                                isProcessed = true;
                                var val = valueProp.GetValue(item);
                                if (val != null  && !string.IsNullOrWhiteSpace(val.ToString()) && val.GetType().ToString() != val.ToString() && !value.GetType().IsSubclassOf(typeof(Stream)))
                                {
                                    serverVariables.Add(prefix + keyProp.GetValue(item), val.ToString());
                                }
                            }
                        }
	                    catch
	                    {
		                    // ignored
	                    }
                    }
                }
                if (!isProcessed)
                {
                    try
                    {
                        if (value != null && !string.IsNullOrWhiteSpace(value.ToString()) && value.GetType().ToString() != value.ToString() && !value.GetType().IsSubclassOf(typeof(Stream))) 
                            serverVariables.Add(prefix + prop.Name, value?.ToString());
                    }
	                catch
	                {
		                // ignored
	                }
                }
            }
        }


		/// <summary>
		/// Gets the <see cref="Exception"/> instance used to initialize this
		/// instance.
		/// </summary>
		/// <remarks>
		/// This is a run-time property only that is not written or read 
		/// during XML serialization via <see cref="ErrorXml.Decode"/> and 
		/// <see cref="ErrorXml.Encode(Error,XmlWriter)"/>.
		/// </remarks>
        [JsonIgnore]
		public Exception Exception { get; }

		/// <summary>
		/// Gets or sets the name of application in which this error occurred.
		/// </summary>

		public string ApplicationName
        { 
            get => _applicationName ?? string.Empty;
		    set => _applicationName = value;
	    }

        /// <summary>
        /// Gets or sets name of host machine where this error occurred.
        /// </summary>
        
        public string HostName
        { 
	        get => _hostName ?? Environment.GetEnvironmentVariable("COMPUTERNAME") ?? Environment.GetEnvironmentVariable("HOSTNAME");
	        set => _hostName = value;
        }

        /// <summary>
        /// Gets or sets the type, class or category of the error.
        /// </summary>
        
        public string Type
        { 
            get => _typeName ?? string.Empty;
	        set => _typeName = value;
        }

        /// <summary>
        /// Gets or sets the source that is the cause of the error.
        /// </summary>
        
        public string Source
        { 
            get => _source ?? string.Empty;
	        set => _source = value;
        }

        /// <summary>
        /// Gets or sets a brief text describing the error.
        /// </summary>
        
        public string Message 
        { 
            get => _message ?? string.Empty;
	        set => _message = value;
        }

        /// <summary>
        /// Gets or sets a detailed text describing the error, such as a
        /// stack trace.
        /// </summary>

        public string Detail
        { 
            get => _detail ?? string.Empty;
	        set => _detail = value;
        }

        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the user logged into the application at the time 
        /// of the error.
        /// </summary>
        
        public string User 
        { 
	        get => _user ?? Environment.GetEnvironmentVariable("USERDOMAIN") ?? Environment.GetEnvironmentVariable("USERNAME") ?? string.Empty;

	        set => _user = value;
        }

		/// <summary>
		/// Gets or sets the date and time (in local time) at which the 
		/// error occurred.
		/// </summary>

		public DateTime Time { get; set; }

		/// <summary>
		/// Gets or sets the HTTP status code of the output returned to the 
		/// client for the error.
		/// </summary>
		/// <remarks>
		/// For cases where this value cannot always be reliably determined, 
		/// the value may be reported as zero.
		/// </remarks>

		public int StatusCode { get; set; }

        public string Method { get; set; }
        public string Client { get; set; }

        /// <summary>
        /// Gets or sets the HTML message generated by the web host (ASP.NET) 
        /// for the given error.
        /// </summary>

        public string WebHostHtmlMessage
        {
            get => _webHostHtmlMessage ?? string.Empty;
	        set => _webHostHtmlMessage = value;
        }

        /// <summary>
        /// Gets a collection representing the Web server variables
        /// captured as part of diagnostic data for the error.
        /// </summary>
        [JsonIgnore]
        public NameValueCollection ServerVariablesItems => FaultIn(ref _serverVariables);
        public Dictionary<string, string> ServerVariables 
            => ServerVariablesItems.AllKeys.ToDictionary(k => k, k => ServerVariablesItems[k]);

        [JsonIgnore]
        public NameValueCollection ConnectionItems => FaultIn(ref _connection);
        public Dictionary<string, string> Connection 
            => ConnectionItems.AllKeys.ToDictionary(k => k, k => ConnectionItems[k]);

        [JsonIgnore]
        public NameValueCollection UserItems => FaultIn(ref _userData);
        public Dictionary<string, string> UserData => UserItems.AllKeys.ToDictionary(k => k, k => UserItems[k]);

        [JsonIgnore]
        public NameValueCollection SessionItems => FaultIn(ref _session);
        public Dictionary<string, string> Session => SessionItems.AllKeys.ToDictionary(k => k, k => SessionItems[k]);

        [JsonIgnore]
        public NameValueCollection ItemsData => FaultIn(ref _items);
        public Dictionary<string, string> Items => ItemsData.AllKeys.ToDictionary(k => k, k => ItemsData[k]);


        [JsonIgnore]
        public NameValueCollection HeaderItems => FaultIn(ref _header);
        public Dictionary<string, string> Header 
            => HeaderItems.AllKeys.ToDictionary(k => k, k => HeaderItems[k]);
        
        /// <summary>
        /// Gets a collection representing the Web query string variables
        /// captured as part of diagnostic data for the error.
        /// </summary>
        
        [JsonIgnore]
        public NameValueCollection QueryStringItems => FaultIn(ref _queryString);
        public Dictionary<string, string> QueryString 
            => QueryStringItems.AllKeys.ToDictionary(k => k, k => QueryStringItems[k]);

	    /// <summary>
        /// Gets a collection representing the form variables captured as 
        /// part of diagnostic data for the error.
        /// </summary>
        [JsonIgnore]
        public NameValueCollection FormItems => FaultIn(ref _form);
        public Dictionary<string, string> From 
            => FormItems.AllKeys.ToDictionary(k => k, k => FormItems[k]);

	    /// <summary>
        /// Gets a collection representing the client cookies
        /// captured as part of diagnostic data for the error.
        /// </summary>
        [JsonIgnore]
        public NameValueCollection CookiesItems => FaultIn(ref _cookies);
        public Dictionary<string, string> Cookies 
            => CookiesItems.AllKeys.ToDictionary(k => k, k => CookiesItems[k]);

        public string Version { get; set; }

        /// <summary>
        /// Returns the value of the <see cref="Message"/> property.
        /// </summary>

        public override string ToString()
        {
            return Message;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
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
            copy._header = CopyCollection(_header);
            copy._connection = CopyCollection(_connection);
            copy._userData = CopyCollection(_userData);
            copy._items = CopyCollection(_items);
            copy._session = CopyCollection(_session);

            return copy;
        }

	    public Error Clone()
	    {
		    return (Error) ((ICloneable) this).Clone();
	    }

        private static NameValueCollection CopyCollection(NameValueCollection collection)
        {
            if (collection == null || collection.Count == 0)
                return null;

            return new NameValueCollection(collection);
        }
        private static NameValueCollection CopyCollection(IEnumerable<KeyValuePair<String, StringValues>> collection)
        {
	        // ReSharper disable once PossibleMultipleEnumeration
	        if (collection == null || !collection.Any())
		        return null;
	        // ReSharper disable once PossibleMultipleEnumeration
	        var keyValuePairs = collection as KeyValuePair<string, StringValues>[] ?? collection.ToArray();
	        if (!keyValuePairs.Any())
                return null;
            var col = new NameValueCollection();
            foreach (var pair in keyValuePairs)
            {
                col.Add(pair.Key, pair.Value);
            }

            return col;
        }

        private static NameValueCollection CopyCollection(IRequestCookieCollection cookies)
        {
            if (cookies == null || cookies.Count == 0)
                return null;

            var copy = new NameValueCollection(cookies.Count);

            foreach (var cookie in cookies)
            {

                //
                // NOTE: We drop the Path and Domain properties of the 
                // cookie for sake of simplicity.
                //

                copy.Add(cookie.Key, cookie.Value);
            }

            return copy;
        }

        private static NameValueCollection FaultIn(ref NameValueCollection collection)
        {
	        return collection ?? (collection = new NameValueCollection());
        }
    }
    public class UserAgent
    {
        private static MemoryCacheOptions cacheOptions = new MemoryCacheOptions();
        private static IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        public UserAgent()
        {
            cacheOptions.SizeLimit = 10000;
        }

        public static UA Parse(string userAgentString)
        {
            userAgentString = (userAgentString.Length > 512) ? userAgentString.Trim().Substring(0, 512) : userAgentString.Trim();
            return cache.GetOrCreate(userAgentString, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(3);
                entry.Size = 1;
                return new UA(userAgentString);
            });
        }

        public UA ParseUserAgent(string userAgentString)
        {
            userAgentString = (userAgentString.Length > 512) ? userAgentString.Trim().Substring(0, 512) : userAgentString.Trim();
            return cache.GetOrCreate(userAgentString, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(3);
                entry.Size = 1;
                return new UA(userAgentString);
            });
        }

    }
}
