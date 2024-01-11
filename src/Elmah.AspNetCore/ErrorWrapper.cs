using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace Elmah.AspNetCore;

[Serializable]
internal class ErrorWrapper
{
    private static readonly Regex Codes = new Regex(
        @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private static readonly Regex Keys = new Regex(
        @"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|Keys)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|Keys)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|Keys)|zz)|mt(50|p1|Keys )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|Keys\-|Keys )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.codes|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-Keys)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private static readonly List<string> Crawlers = new List<string>
    {
        "googlebot", "bingbot", "yandexbot", "ahrefsbot", "msnbot", "linkedinbot", "exabot", "compspybot",
        "yesupbot", "paperlibot", "tweetmemebot", "semrushbot", "gigabot", "voilabot", "adsbot-google",
        "botlink", "alkalinebot", "araybot", "undrip bot", "borg-bot", "boxseabot", "yodaobot", "admedia bot",
        "ezooms.bot", "confuzzledbot", "coolbot", "internet cruiser robot", "yolinkbot", "diibot", "musobot",
        "dragonbot", "elfinbot", "wikiobot", "twitterbot", "contextad bot", "hambot", "iajabot", "news bot",
        "irobot", "socialradarbot", "ko_yappo_robot", "skimbot", "psbot", "rixbot", "seznambot", "careerbot",
        "simbot", "solbot", "mail.ru_bot", "spiderbot", "blekkobot", "bitlybot", "techbot", "void-bot",
        "vwbot_k", "diffbot", "friendfeedbot", "archive.org_bot", "woriobot", "crystalsemanticsbot", "wepbot",
        "spbot", "tweetedtimes bot", "mj12bot", "who.is bot", "psbot", "robot", "jbot", "bbot", "bot"
    };

    private readonly Error _error = default!;

    public ErrorWrapper()
    {
    }

    public ErrorWrapper(Error error, string[]? sourcePath)
    {
        _error = error ?? throw new ArgumentNullException(nameof(error));
        HtmlMessage = ErrorDetailHelper.MarkupStackTrace(_error.Detail, out var srcList);
        if (srcList?.Any() == true)
        {
            Sources = srcList.Select(i
                    => ErrorDetailHelper.GetStackFrameSourceCodeInfo(sourcePath, i.Method, i.Type, i.Source,
                        i.Line))
                .Where(i => !string.IsNullOrEmpty(i.ContextCode))
                .ToList();
        }
    }

    public List<StackFrameSourceCodeInfo>? Sources { get; private set; }

    [XmlElement("ApplicationName")]
    public string ApplicationName
    {
        get => _error.ApplicationName;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlElement("HostName")]
    public string HostName
    {
        get => _error.HostName;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlElement("Type")]
    public string Type
    {
        get => _error.Type;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlElement("Body")]
    public string? Body
    {
        get => _error.Body;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlElement("Source")]
    public string Source
    {
        get => _error.Source;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlElement("Message")]
    public string Message
    {
        get => _error.Message;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlElement("Detail")]
    public string Detail
    {
        get => _error.Detail;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlElement("User")]
    public string User
    {
        get => _error.User;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }
    
    [XmlElement("Time")]
    public DateTime Time
    {
        get => _error.Time;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlElement("StatusCode")]
    public int? StatusCode
    {
        get => _error.StatusCode == 0 ? (int?) null : _error.StatusCode;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlIgnore] public string? HtmlMessage { get; set; }

    public bool IsMobile
    {
        get
        {
            var u = _error.ServerVariables["Header_User-Agent"];
            if (string.IsNullOrEmpty(u))
            {
                return false;
            }

            return Codes.IsMatch(u) || Keys.IsMatch(u[..4]);
        }
    }

    public string? Os
    {
        get
        {
            var userAgent = _error.ServerVariables["Header_User-Agent"];
            if (string.IsNullOrEmpty(userAgent))
            {
                return null;
            }

            if (userAgent.Contains("Windows"))
            {
                return "Windows";
            }

            if (userAgent.Contains("Android"))
            {
                return "Android";
            }

            if (userAgent.Contains("Linux"))
            {
                return "Linux";
            }

            if (userAgent.Contains("iPhone"))
            {
                return "iPhone";
            }

            if (userAgent.Contains("iPad"))
            {
                return "iPhone";
            }

            if (userAgent.Contains("Macintosh"))
            {
                return "Macintosh";
            }

            return null;
        }
    }

    public string? Browser
    {
        get
        {
            var userAgent = _error.ServerVariables["Header_User-Agent"];
            if (string.IsNullOrEmpty(userAgent))
            {
                return null;
            }

            if (Crawlers.Exists(x => userAgent.Contains(x)))
            {
                return "Bot";
            }

            if (userAgent.Contains("Chrome"))
            {
                return "Chrome";
            }

            if (userAgent.Contains("Firefox"))
            {
                return "Firefox";
            }

            if (userAgent.Contains("Safari") || userAgent.Contains("AppleWebKit"))
            {
                return "Safari";
            }

            if (userAgent.Contains("OP"))
            {
                return "Opera";
            }

            if (userAgent.Contains("Edge"))
            {
                return "Edge";
            }

            if (userAgent.Contains("AppleWebKit"))
            {
                return "AndroidBrowser";
            }

            if (userAgent.Contains("Vivaldi"))
            {
                return "Vivaldi";
            }

            if (userAgent.Contains("Brave"))
            {
                return "Brave";
            }

            if (userAgent.Contains("MSIE") || userAgent.Contains("rv:"))
            {
                return "MSIE";
            }

            return "Generic";
        }
    }

    public string Severity
    {
        get
        {
            if (_error.StatusCode == 0)
            {
                return "Error";
            }

            if (_error.StatusCode < 200)
            {
                return "Info";
            }

            if (_error.StatusCode < 400)
            {
                return "Success";
            }

            if (_error.StatusCode < 500)
            {
                return "Warning";
            }

            return "Error";
        }
    }

    [XmlElement("Method")]
    public string Method
    {
        get => _error.ServerVariables["Method"]!;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlElement("Url")]
    public string Url
    {
        get => _error.ServerVariables["PathBase"] + _error.ServerVariables["Path"];
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlElement("Client")]
    public string? Client
    {
        get => _error.ServerVariables["Connection_RemoteIpAddress"];
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public string? Version => _error.ServerVariables["Version"];

    [XmlIgnore]
    public List<ErrorLogMessageWrapper> MessageLog
    {
        get => GetMessageLog();
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    private List<ErrorLogMessageWrapper> GetMessageLog()
    {
        var result = _error.MessageLog.Select(x => new ErrorLogMessageWrapper(x)).ToList();
        foreach (var param in _error.Params)
        {
            result.Add(new ErrorLogMessageWrapper(new XmlLogMessage
            {
                TimeStamp = param.TimeStamp,
                Level = LogLevel.Information,
                Message = $"Method {param.TypeName}.{param.MemberName} call with parameters:"
            }, param.Params));
        }

        return result.OrderBy(i => i.TimeStamp).ToList();
    }

    [XmlIgnore]
    public List<ElmahLogSqlEntry> SqlLog
    {
        get => _error.SqlLog.ToList();
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlIgnore]
    public List<ElmahLogParamEntry> Params
    {
        get => _error.Params.ToList();
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public SerializableDictionary<string, string?> Form
    {
        get =>
            _error.Form.AllKeys
                .Where(i => i != "$request-body")
                .ToSerializableDictionary(k => k!, k => _error.Form[k]);
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public SerializableDictionary<string, string?> QueryString
    {
        get =>
            _error.QueryString.AllKeys
                .ToSerializableDictionary(k => k!, k => _error.QueryString[k]);
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public SerializableDictionary<string, string?> Cookies
    {
        get =>
            _error.Cookies.AllKeys
                .ToSerializableDictionary(k => k!, k => _error.Cookies[k]);
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    [XmlElement("Header")]
    public SerializableDictionary<string, string?> Header
    {
        get
        {
            return _error.ServerVariables.AllKeys
                .Where(i => !string.IsNullOrEmpty(i))
                .Where(i => i!.StartsWith("Header_"))
                .ToSerializableDictionary(k => k!["Header_".Length..], k => _error.ServerVariables[k]);
        }
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public SerializableDictionary<string, string?> Connection
    {
        get =>
            _error.ServerVariables.AllKeys
                .Where(i => !string.IsNullOrEmpty(i))
                .Where(i => i!.StartsWith("Connection_"))
                .Where(i => i!.Contains("Port") && _error.ServerVariables[i] != "0") //ignore empty
                .ToSerializableDictionary(k => k!["Connection_".Length..], k => _error.ServerVariables[k]);
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public SerializableDictionary<string, string?> Items
    {
        get =>
            _error.ServerVariables.AllKeys
                .Where(i => !string.IsNullOrEmpty(i))
                .Where(i => i!.StartsWith("Items_"))
                .ToSerializableDictionary(k => k!["Items_".Length..], k => _error.ServerVariables[k]);
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public SerializableDictionary<string, string?> Session
    {
        get =>
            _error.ServerVariables.AllKeys
                .Where(i => !string.IsNullOrEmpty(i))
                .Where(i => i!.StartsWith("Session_"))
                .ToSerializableDictionary(k => k!["Session_".Length..], k => _error.ServerVariables[k]);
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public SerializableDictionary<string, string?> UserData
    {
        get =>
            _error.ServerVariables.AllKeys
                .Where(i => !string.IsNullOrEmpty(i))    
                .Where(i => i!.StartsWith("User_"))
                .ToSerializableDictionary(k => k!["User_".Length..], k => _error.ServerVariables[k]);
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public SerializableDictionary<string, string?> ServerVariables
    {
        get
        {
            var keyWords = new[] {"User_", "Header_", "Connection_", "Items_", "Session_"};
            return _error.ServerVariables.AllKeys
                .Where(i => !string.IsNullOrEmpty(i))
                .Where(i => !keyWords.Any(i!.StartsWith))
                .ToSerializableDictionary(k => k!, k => _error.ServerVariables[k]);
        }
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    internal sealed class ErrorLogMessageWrapper
    {
        private readonly IElmahLogMessage _message;
        private readonly KeyValuePair<string, string>[]? _parameters;

        public ErrorLogMessageWrapper(IElmahLogMessage message, KeyValuePair<string, string>[]? parameters = null)
        {
            _message = message;
            _parameters = parameters;
        }

        public DateTime TimeStamp => _message.TimeStamp;

        public string? Exception => _message.Exception;

        public string? Scope => _message.Scope;

        public LogLevel? Level => _message.Level;

        public string? Message => _message.Render();

        public KeyValuePair<string, string>[]? Params => _parameters;

        // This property is used on client side only - passing it down as initial state
        public bool Collapsed => true;
    }
}