using System;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace ElmahCore
{
    public struct ElmahLogMessageEntry
    {
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public string Scope { get; set; }
        public string Exception { get; set; }
        public LogLevel Level { get; set; }
        [XmlIgnore]
        public bool Collapsed
        {
            get => true;
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }
    }
}