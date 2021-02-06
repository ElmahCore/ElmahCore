using System.Diagnostics;
using System.IO;
using System.Xml;

namespace ElmahCore.Mvc.Xml
{
    #region Imports

    using SystemXmlSerializer = System.Xml.Serialization.XmlSerializer;

    #endregion

    /// <summary>
    ///     Serializes object to and from XML documents.
    /// </summary>
    internal static class XmlSerializer
    {
        public static string Serialize(object obj)
        {
            var sw = new StringWriter();
            Serialize(obj, sw);
            return sw.GetStringBuilder().ToString();
        }

        private static void Serialize(object obj, TextWriter output)
        {
            Debug.Assert(obj != null);
            Debug.Assert(output != null);

            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            settings.CheckCharacters = false;
            settings.OmitXmlDeclaration = true;
            var writer = XmlWriter.Create(output, settings);

            try
            {
                var serializer = new SystemXmlSerializer(obj.GetType());
                serializer.Serialize(writer, obj);
                writer.Flush();
            }
            finally
            {
                writer.Close();
            }
        }
    }
}