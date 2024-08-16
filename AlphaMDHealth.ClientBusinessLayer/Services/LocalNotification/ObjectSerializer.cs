using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace AlphaMDHealth.ClientBusinessLayer
{
    /// <summary>
    ///
    /// </summary>
    public class ObjectSerializer<T>
    {
        /// <summary>
        /// Deserialize Returning Data.
        /// </summary>
        /// <param name="returningData"></param>
        /// <returns></returns>
        public T DeserializeObject(string returningData)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StringReader stringReader = new StringReader(returningData))
            {
                using (XmlReader xmlReader = XmlReader.Create(stringReader,
                    new XmlReaderSettings
                    {
                        DtdProcessing = DtdProcessing.Prohibit
                    }))
                {
                    T notification = (T)xmlSerializer.Deserialize(xmlReader);
                    return notification;
                }
            }
        }

        /// <summary>
        /// Serialize Returning Data
        /// </summary>
        /// <param name="returningData"></param>
        /// <returns></returns>
        public string SerializeObject(T returningData)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StringWriter stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, returningData);
                return stringWriter.ToString();
            }
        }
    }
}