using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace ElmahCore.Mvc.Xml
{
    internal static class RssXml
    {
        public static XElement Rss(string title, string link, string description, IEnumerable<XElement> items)
        {
            return
                new XElement("rss",
                    new XAttribute("version", "0.91"), // http://backend.userland.com/rss091
                    new XElement("channel",
                        new XElement("title", title),
                        new XElement("link", link),
                        new XElement("description", description),
                        new XElement("language", "en"),
                        items));
        }

        public static XElement Item(string title, string description, DateTime pubDate, string link = null)
        {
            return
                new XElement("item",
                    new XElement("title", title),
                    new XElement("description", description),
                    new XElement("pubDate", pubDate.ToUniversalTime().ToString("r", DateTimeFormatInfo.InvariantInfo)),
                    link != null ? new XElement("link", link) : null);
        }
    }
}