using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

namespace ElmahCore
{
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only 
    /// way of generating streams or files containing JSON Text according
    /// to the grammar rules laid out in 
    /// <a href="http://www.ietf.org/rfc/rfc4627.txt">RFC 4627</a>.
    /// </summary>
    /// <remarks>
    /// This class supports ELMAH and is not intended to be used directly 
    /// from your code. It may be modified or removed in the future without 
    /// notice. It has public accessibility for testing purposes. If you
    /// need a general-purpose JSON Text encoder, consult
    /// <a href="http://www.json.org/">JSON.org</a> for implementations
    /// or use classes available from the Microsoft .NET Framework.
    /// </remarks>

    internal sealed class JsonTextWriter
    {
        private readonly TextWriter _writer;
        private readonly int[] _counters;
        private readonly char[] _terminators;
        private string _memberName;

        public JsonTextWriter(TextWriter writer)
        {
            Debug.Assert(writer != null);
            _writer = writer;
            const int levels = 10 + /* root */ 1;
            _counters = new int[levels];
            _terminators = new char[levels];
        }

        public int Depth { get; private set; }

        private int ItemCount
        {
            get => _counters[Depth];
	        set => _counters[Depth] = value;
        }

        private char Terminator
        {
            get => _terminators[Depth];
	        set => _terminators[Depth] = value;
        }

        public JsonTextWriter Object()
        {
            return StartStructured("{", "}");
        }

	    public JsonTextWriter Array()
        {
            return StartStructured("[", "]");
        }

	    public JsonTextWriter Pop()
        {
            return EndStructured();
        }

        public JsonTextWriter Member(string name)
        {
	        if (_memberName != null) throw new InvalidOperationException("Missing member value.");
            _memberName = name ?? throw new ArgumentNullException(nameof(name));
            return this;
        }

        private JsonTextWriter Write(string text)
        {
            return WriteImpl(text, /* raw */ false);
        }

        private JsonTextWriter WriteEnquoted(string text)
        {
            return WriteImpl(text, /* raw */ true);
        }

        private JsonTextWriter WriteImpl(string text, bool raw)
        {
            Debug.Assert(raw || !string.IsNullOrEmpty(text));

            if (Depth == 0 && (text.Length > 1 || (text[0] != '{' && text[0] != '[')))
                throw new InvalidOperationException();

            var writer = _writer;

            if (ItemCount > 0)
                writer.Write(',');   

            var name = _memberName;
            _memberName = null;

            if (name != null)
            {
                writer.Write(' ');
                Enquote(name, writer);
                writer.Write(':');
            }

            if (Depth > 0) 
                writer.Write(' ');
            
            if (raw) 
                Enquote(text, writer); 
            else 
                writer.Write(text);
            
            ItemCount = ItemCount + 1;

            return this;
        }

        public JsonTextWriter Number(int value)
        {
            return Write(value.ToString(CultureInfo.InvariantCulture));
        }

        public JsonTextWriter String(string str)
        {
            return str == null ? Null() : WriteEnquoted(str);
        }

        public JsonTextWriter Null()
        {
            return Write("null");
        }

	    public JsonTextWriter String(DateTime time)
        {
            return String(XmlConvert.ToString(time, XmlDateTimeSerializationMode.Utc));
        }

        private JsonTextWriter StartStructured(string start, string end)
        {
            if (Depth + 1 == _counters.Length)
                throw new Exception();

            Write(start);
            Depth++;
            Terminator = end[0];
            return this;
        }

        private JsonTextWriter EndStructured()
        {
            if (Depth - 1 < 0)
                throw new Exception();

            _writer.Write(' ');
            _writer.Write(Terminator);
            ItemCount = 0;
            Depth--;
            return this;
        }

        private static void Enquote(string s, TextWriter writer)
        {
            Debug.Assert(writer != null);

            var length = (s ?? string.Empty).Length;

            writer.Write('"');

            var ch = '\0';

            for (var index = 0; index < length; index++)
            {
                var last = ch;

                Debug.Assert(s != null);
                ch = s[index];

                switch (ch)
                {
                    case '\\':
                    case '"':
                    {
                        writer.Write('\\');
                        writer.Write(ch);
                        break;
                    }

                    case '/':
                    {
                        if (last == '<')
                            writer.Write('\\');
                        writer.Write(ch);
                        break;
                    }

                    case '\b': writer.Write("\\b"); break;
                    case '\t': writer.Write("\\t"); break;
                    case '\n': writer.Write("\\n"); break;
                    case '\f': writer.Write("\\f"); break;
                    case '\r': writer.Write("\\r"); break;

                    default:
                    {
                        if (ch < ' ')
                        {
                            writer.Write("\\u");
                            writer.Write(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            writer.Write(ch);
                        }

                        break;
                    }
                }
            }

            writer.Write('"');
        }
    }
}