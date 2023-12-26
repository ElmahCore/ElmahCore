using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace ElmahCore;

/// <summary>
///     Responsible for encoding and decoding the XML representation of
///     an <see cref="Error" /> object.
/// </summary>
public static class ErrorXml
{
    /// <summary>
    ///     Decodes an <see cref="Error" /> object from its default XML
    ///     representation.
    /// </summary>
    public static Error DecodeString(Guid id, string xml)
    {
        using (var sr = new StringReader(xml))
        using (var reader = XmlReader.Create(sr, new XmlReaderSettings() { CheckCharacters = false }))
        {
            if (!reader.IsStartElement("error"))
            {
                throw new ApplicationException("The error XML is not in the expected format.");
            }

            return Decode(id, reader);
        }
    }

    /// <summary>
    ///     Decodes an <see cref="Error" /> object from its XML representation.
    /// </summary>
    public static Error Decode(Guid id, XmlReader reader)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        if (!reader.IsStartElement())
        {
            throw new ArgumentException("Reader is not positioned at the start of an element.", nameof(reader));
        }

        //
        // Read out the attributes that contain the simple
        // typed state.
        //

        var error = new Error()
        {
            Id = id
        };

        ReadXmlAttributes(reader, error);

        //
        // Move past the element. If it's not empty, then
        // read also the inner XML that contains complex
        // types like collections.
        //

        var isEmpty = reader.IsEmptyElement;
        reader.Read();

        if (!isEmpty)
        {
            ReadInnerXml(reader, error);
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Skip();
            }

            reader.ReadEndElement();
        }

        return error;
    }

    /// <summary>
    ///     Reads the error data in XML attributes.
    /// </summary>
    private static void ReadXmlAttributes(XmlReader reader, Error error)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        if (!reader.IsStartElement())
        {
            throw new ArgumentException("Reader is not positioned at the start of an element.", nameof(reader));
        }

        error.ApplicationName = reader.GetAttribute("application");
        error.HostName = reader.GetAttribute("host");
        error.Type = reader.GetAttribute("type");
        error.Message = reader.GetAttribute("message");
        error.Source = reader.GetAttribute("source");
        error.Detail = reader.GetAttribute("detail");
        error.User = reader.GetAttribute("user");
        var timeString = reader.GetAttribute("time") ?? string.Empty;
        error.Time = timeString.Length == 0
            ? new DateTime()
            : XmlConvert.ToDateTime(timeString, XmlDateTimeSerializationMode.Local);
        var statusCodeString = reader.GetAttribute("statusCode") ?? string.Empty;
        error.StatusCode = statusCodeString.Length == 0 ? 0 : XmlConvert.ToInt32(statusCodeString);
    }

    /// <summary>
    ///     Reads the error data in child nodes.
    /// </summary>
    private static void ReadInnerXml(XmlReader reader, Error error)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        //
        // Loop through the elements, reading those that we
        // recognize. If an unknown element is found then
        // this method bails out immediately without
        // consuming it, assuming that it belongs to a subclass.
        //

        while (reader.IsStartElement())
        {
            //
            // Optimization Note: This block could be re-wired slightly 
            // to be more efficient by not causing a collection to be
            // created if the element is going to be empty.
            //

            NameValueCollection collection;
            if (reader.Name == "paramsLog")
            {
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                }
                else
                {
                    UpcodeToParams(reader, error.Params);
                }
            }
            if (reader.Name == "sqlLog")
            {
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                }
                else
                {
                    UpcodeToSqlLog(reader, error.SqlLog);
                }
            }
            else if (reader.Name == "messageLog")
            {
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                }
                else
                {
                    UpcodeToLog(reader, error.MessageLog);
                }
            }
            else
            {
                switch (reader.Name)
                {
                    case "serverVariables":
                        collection = error.ServerVariables;
                        break;
                    case "queryString":
                        collection = error.QueryString;
                        break;
                    case "form":
                        collection = error.Form;
                        break;
                    case "cookies":
                        collection = error.Cookies;
                        break;
                    default:
                        reader.Skip();
                        continue;
                }

                if (reader.IsEmptyElement)
                {
                    reader.Read();
                }
                else
                {
                    UpcodeTo(reader, collection);
                }
            }
        }
    }

    /// <summary>
    ///     Encodes the default XML representation of an <see cref="Error" />
    ///     object to a string.
    /// </summary>
    public static string EncodeString(Error error)
    {
        var sw = new StringWriter();

        using var writer = XmlWriter.Create(sw, new XmlWriterSettings
        {
            Indent = true,
            NewLineOnAttributes = true,
            CheckCharacters = false,
            OmitXmlDeclaration = true // see issue #120: http://code.google.com/p/elmah/issues/detail?id=120
        });
        writer.WriteStartElement("error");
        Encode(error, writer);
        writer.WriteEndElement();
        writer.Flush();

        return sw.ToString();
    }

    /// <summary>
    ///     Encodes the XML representation of an <see cref="Error" /> object.
    /// </summary>
    public static void Encode(Error error, XmlWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        if (writer.WriteState != WriteState.Element)
        {
            throw new ArgumentException("Writer is not in the expected Element state.", nameof(writer));
        }

        //
        // Write out the basic typed information in attributes
        // followed by collections as inner elements.
        //

        WriteXmlAttributes(error, writer);
        WriteInnerXml(error, writer);
    }

    /// <summary>
    ///     Writes the error data that belongs in XML attributes.
    /// </summary>
    private static void WriteXmlAttributes(Error error, XmlWriter writer)
    {
        Debug.Assert(error != null);
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        WriteXmlAttribute(writer, "application", error.ApplicationName);
        WriteXmlAttribute(writer, "host", error.HostName);
        WriteXmlAttribute(writer, "type", error.Type);
        WriteXmlAttribute(writer, "message", error.Message);
        WriteXmlAttribute(writer, "source", error.Source);
        WriteXmlAttribute(writer, "detail", error.Detail);
        WriteXmlAttribute(writer, "user", error.User);
        if (error.Time != DateTime.MinValue)
        {
            WriteXmlAttribute(writer, "time",
                XmlConvert.ToString(error.Time.ToUniversalTime(), @"yyyy-MM-dd\THH:mm:ss.fffffff\Z"));
        }

        if (error.StatusCode != 0)
        {
            WriteXmlAttribute(writer, "statusCode", XmlConvert.ToString(error.StatusCode));
        }
    }

    /// <summary>
    ///     Writes the error data that belongs in child nodes.
    /// </summary>
    private static void WriteInnerXml(Error error, XmlWriter writer)
    {
        Debug.Assert(error != null);
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        WriteCollection(writer, "serverVariables", error.ServerVariables);
        WriteCollection(writer, "queryString", error.QueryString);
        WriteCollection(writer, "form", error.Form);
        WriteCollection(writer, "cookies", error.Cookies);
        WriteMessageLog(writer, "messageLog", error.MessageLog);
        WriteSqlLog(writer, "sqlLog", error.SqlLog);
        WriteParamsLog(writer, "paramsLog", error.Params);
    }

    private static void WriteParamsLog(XmlWriter writer, string name, IEnumerable<ElmahLogParamEntry> log)
    {
        if (log == null)
        {
            return;
        }

        var list = log.ToList();
        if (list.Count == 0)
        {
            return;
        }

        writer.WriteStartElement(name);
        foreach (var entry in list)
        {
            writer.WriteStartElement("parameter");
            WriteXmlAttribute(writer, "time-stamp",
                XmlConvert.ToString(entry.TimeStamp.ToUniversalTime(), @"yyyy-MM-dd\THH:mm:ss.fffffff\Z"));
            WriteXmlAttribute(writer, "type-name", entry.TypeName);
            WriteXmlAttribute(writer, "member-name", entry.MemberName);
            WriteXmlAttribute(writer, "file", entry.File);
            WriteXmlAttribute(writer, "line", entry.Line.ToString());
            foreach (var entryParam in entry.Params)
            {
                writer.WriteStartElement("param");
                WriteXmlAttribute(writer, "name", entryParam.Key);
                WriteXmlAttribute(writer, "value", entryParam.Value);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }
    private static void WriteSqlLog(XmlWriter writer, string name, IEnumerable<ElmahLogSqlEntry> log)
    {
        if (log == null)
        {
            return;
        }

        var list = log.ToList();
        if (list.Count == 0)
        {
            return;
        }

        writer.WriteStartElement(name);
        foreach (var entry in list)
        {
            writer.WriteStartElement("sql");
            WriteXmlAttribute(writer, "command-type", entry.CommandType);
            WriteXmlAttribute(writer, "sql-text", entry.SqlText);
            WriteXmlAttribute(writer, "time-stamp",
               XmlConvert.ToString(entry.TimeStamp.ToUniversalTime(), @"yyyy-MM-dd\THH:mm:ss.fffffff\Z"));
            WriteXmlAttribute(writer, "duration", entry.DurationMs.ToString());
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteMessageLog(XmlWriter writer, string name, IEnumerable<ElmahLogMessageEntry> log)
    {
        if (log == null)
        {
            return;
        }

        var list = log.ToList();
        if (list.Count == 0)
        {
            return;
        }

        writer.WriteStartElement(name);
        foreach (var entry in list)
        {
            writer.WriteStartElement("message");
            WriteXmlAttribute(writer, "level", entry.Level.ToString());
            WriteXmlAttribute(writer, "exception", entry.Exception);
            WriteXmlAttribute(writer, "time-stamp",
                XmlConvert.ToString(entry.TimeStamp.ToUniversalTime(), @"yyyy-MM-dd\THH:mm:ss.fffffff\Z"));
            WriteXmlAttribute(writer, "scope", entry.Scope);
            WriteXmlAttribute(writer, "message", entry.Message);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteCollection(XmlWriter writer, string name, NameValueCollection collection)
    {
        Debug.Assert(writer != null);

        if (collection == null || collection.Count == 0)
        {
            return;
        }

        writer.WriteStartElement(name);
        Encode(collection, writer);
        writer.WriteEndElement();
    }

    private static void WriteXmlAttribute(XmlWriter writer, string name, string? value)
    {
        Debug.Assert(writer != null);

        if (!string.IsNullOrEmpty(value))
        {
            writer.WriteAttributeString(name, value);
        }
    }

    /// <summary>
    ///     Encodes an XML representation for a
    ///     <see cref="NameValueCollection" /> object.
    /// </summary>
    private static void Encode(NameValueCollection collection, XmlWriter writer)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        if (collection.Count == 0)
        {
            return;
        }

        //
        // Write out a named multi-value collection as follows 
        // (example here is the ServerVariables collection):
        //
        //      <item name="HTTP_URL">
        //          <value string="/myapp/somewhere/page.aspx" />
        //      </item>
        //      <item name="QUERY_STRING">
        //          <value string="a=1&amp;b=2" />
        //      </item>
        //      ...
        //

        foreach (string key in collection.Keys)
        {
            writer.WriteStartElement("item");
            writer.WriteAttributeString("name", key);

            var values = collection.GetValues(key);

            if (values != null)
            {
                foreach (var value in values)
                {
                    writer.WriteStartElement("value");
                    writer.WriteAttributeString("string", value);
                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
        }
    }

    /// <summary>
    ///     Updates an existing <see cref="NameValueCollection" /> object from
    ///     its XML representation.
    /// </summary>
    private static void UpcodeTo(XmlReader reader, NameValueCollection collection)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        Debug.Assert(!reader.IsEmptyElement);
        reader.Read();

        //
        // Add entries into the collection as <item> elements
        // with child <value> elements are found.
        //

        while (reader.NodeType != XmlNodeType.EndElement)
        {
            if (reader.IsStartElement("item"))
            {
                var name = reader.GetAttribute("name");
                var isNull = reader.IsEmptyElement;

                reader.Read(); // <item>

                if (!isNull)
                {
                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        if (reader.IsStartElement("value")) // <value ...>
                        {
                            var value = reader.GetAttribute("string");
                            collection.Add(name, value);
                            if (reader.IsEmptyElement)
                            {
                                reader.Read();
                            }
                            else
                            {
                                reader.Read();
                                while (reader.NodeType != XmlNodeType.EndElement)
                                {
                                    reader.Skip();
                                }

                                reader.ReadEndElement();
                            }
                        }
                        else
                        {
                            reader.Skip();
                        }

                        reader.MoveToContent();
                    }

                    reader.ReadEndElement(); // </item>
                }
                else
                {
                    collection.Add(name, null);
                }
            }
            else
            {
                reader.Skip();
            }

            reader.MoveToContent();
        }

        reader.ReadEndElement();
    }

    private static void UpcodeToLog(XmlReader reader, ICollection<ElmahLogMessageEntry> log)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        if (log == null)
        {
            throw new ArgumentNullException(nameof(log));
        }

        Debug.Assert(!reader.IsEmptyElement);
        reader.Read();

        while (reader.NodeType != XmlNodeType.EndElement)
        {
            if (reader.IsStartElement("message"))
            {
                var entry = new ElmahLogMessageEntry
                {
                    Level = GetLogLevel(reader.GetAttribute("level")),
                    Exception = reader.GetAttribute("exception"),
                    TimeStamp = LoadTime(reader.GetAttribute("time-stamp") ?? string.Empty),
                    Scope = reader.GetAttribute("scope"),
                    Message = reader.GetAttribute("message")
                };

                log.Add(entry);

                reader.Read(); // <item>
            }
            else
            {
                reader.Skip();
            }

            reader.MoveToContent();
        }

        reader.ReadEndElement();
    }

    private static void UpcodeToParams(XmlReader reader, ICollection<ElmahLogParamEntry> log)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        if (log == null)
        {
            throw new ArgumentNullException(nameof(log));
        }

        Debug.Assert(!reader.IsEmptyElement);
        reader.Read();
        
        while (reader.NodeType != XmlNodeType.EndElement)
        {
            if (reader.IsStartElement("parameter"))
            {
                var timeStamp = LoadTime(reader.GetAttribute("time-stamp") ?? string.Empty);
                var list = new List<KeyValuePair<string, string>>();
                var typeName = reader.GetAttribute("type-name");
                var memberName = reader.GetAttribute("member-name");
                var line = int.Parse(reader.GetAttribute("line") ?? "0");

                reader.Read(); 
                
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement("param"))
                    {
                        var data = new KeyValuePair<string, string>(
                            reader.GetAttribute("name")!, reader.GetAttribute("value")!);
                        list.Add(data);
                    }
                    else
                    {
                        reader.Skip();
                    }
                    reader.Read(); // <item>
                }

                if (list.Any())
                {
                    log.Add(new ElmahLogParamEntry(
                        timeStamp,
                        list.ToArray(),
                        typeName,
                        memberName!,
                        reader.GetAttribute("file")!,
                        line));
                }
            }
            else
            {
                reader.Skip();
            }

            reader.MoveToContent();
        }

        reader.ReadEndElement();
    }
    private static DateTime LoadTime(string text)
    {
        return text.Length == 0
            ? new DateTime()
            : XmlConvert.ToDateTime(text, XmlDateTimeSerializationMode.Local);
    }
    
    private static void UpcodeToSqlLog(XmlReader reader, ICollection<ElmahLogSqlEntry> log)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        if (log == null)
        {
            throw new ArgumentNullException(nameof(log));
        }

        Debug.Assert(!reader.IsEmptyElement);
        reader.Read();

        while (reader.NodeType != XmlNodeType.EndElement)
        {
            if (reader.IsStartElement("sql"))
            {
                var entry = new ElmahLogSqlEntry
                {
                    CommandType = reader.GetAttribute("command-type"),
                    SqlText = reader.GetAttribute("sql-text"),
                    TimeStamp = LoadTime(reader.GetAttribute("time-stamp") ?? string.Empty),
                    DurationMs = int.Parse(reader.GetAttribute("duration") ?? "0")
                };

                log.Add(entry);

                reader.Read(); // <item>
            }
            else
            {
                reader.Skip();
            }

            reader.MoveToContent();
        }

        reader.ReadEndElement();
    }

    private static LogLevel GetLogLevel(string? level)
    {
        Enum.TryParse(level, out LogLevel result);
        return result;
    }
}