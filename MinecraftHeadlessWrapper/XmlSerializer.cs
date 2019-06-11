using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace MinecraftHeadlessWrapper
{
    /// <summary>
    /// Pomocné funkce pro (de)serializaci XML.
    /// </summary>
    public static class XmlSerializer
    {
        /// <summary>
        /// Deserializuje XML řetězec do specifického datového modelu.
        /// </summary>
        /// <typeparam name="T">Typ objektu.</typeparam>
        /// <param name="xmlString">XML řetězec.</param>
        public static T Deserialize<T>(string xmlString)
        {
            if (string.IsNullOrEmpty(xmlString)) throw new ArgumentException("Empty Xml String");
            return (T)(new System.Xml.Serialization.XmlSerializer(typeof(T))).Deserialize(new StringReader(xmlString));
        }

        /// <summary>
        /// Serializuje objekt do XML řetězce.
        /// </summary>
        /// <param name="obj">Objekt k serializaci.</param>
        public static string Serialize(object obj)
        {
            if (obj == null) throw new ArgumentException("Null object");
            var sb = new StringBuilder();
            (new System.Xml.Serialization.XmlSerializer(obj.GetType())).Serialize(new StringWriter(sb), obj);
            return sb.ToString();
        }
    }
}
