using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ElmahCore.Mvc
{
    [Serializable()]
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
            int itemCount = info.GetInt32("ItemCount");
            for (int i = 0; i < itemCount; i++)
            {
                KeyValuePair<TKey, TVal> kvp = (KeyValuePair<TKey, TVal>)info.GetValue($"Item{i}", typeof(KeyValuePair<TKey, TVal>));
                this.Add(kvp.Key, kvp.Value);
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ItemCount", this.Count);
            int itemIdx = 0;
            foreach (KeyValuePair<TKey, TVal> kvp in this)
            {
                info.AddValue($"Item{itemIdx}", kvp, typeof(KeyValuePair<TKey, TVal>));
                itemIdx++;
            }
        }

        #endregion
        #region IXmlSerializable Members

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (KeyValuePair<TKey, TVal> kvp in this)
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

        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader) => throw new NotImplementedException();

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema() => null;
        #endregion
    }
}