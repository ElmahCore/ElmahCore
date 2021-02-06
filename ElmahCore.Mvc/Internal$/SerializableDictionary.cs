using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ElmahCore.Mvc
{
    [Serializable]
    public class SerializableDictionary<TKey, TVal> : Dictionary<TKey, TVal>, IXmlSerializable, ISerializable
    {
        #region Constants

        private const string ItemNodeName = "Item";
        private const string KeyNodeName = "Key";
        private const string ValueNodeName = "Value";

        #endregion

        #region Constructors

        public SerializableDictionary()
        {
        }

        public SerializableDictionary(IDictionary<TKey, TVal> dictionary)
            : base(dictionary)
        {
        }

        public SerializableDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
        }

        public SerializableDictionary(int capacity)
            : base(capacity)
        {
        }

        public SerializableDictionary(IDictionary<TKey, TVal> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {
        }

        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {
        }

        #endregion

        #region ISerializable Members

        protected SerializableDictionary(SerializationInfo info, StreamingContext context)
        {
            var itemCount = info.GetInt32("ItemCount");
            for (var i = 0; i < itemCount; i++)
            {
                var kvp = (KeyValuePair<TKey, TVal>) info.GetValue($"Item{i}", typeof(KeyValuePair<TKey, TVal>));
                Add(kvp.Key, kvp.Value);
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ItemCount", Count);
            var itemIdx = 0;
            foreach (var kvp in this)
            {
                info.AddValue($"Item{itemIdx}", kvp, typeof(KeyValuePair<TKey, TVal>));
                itemIdx++;
            }
        }

        #endregion

        #region IXmlSerializable Members

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            foreach (var kvp in this)
            {
                writer.WriteStartElement(ItemNodeName);
                writer.WriteStartElement(KeyNodeName);
                writer.WriteString(kvp.Key?.ToString() ?? "");
                writer.WriteEndElement();
                writer.WriteStartElement(ValueNodeName);
                writer.WriteString(kvp.Value?.ToString() ?? "");
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        #endregion
    }
}