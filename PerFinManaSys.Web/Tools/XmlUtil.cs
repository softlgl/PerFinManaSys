using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NewXml
{
    /// <summary>
    /// Xml序列化与反序列化
    /// </summary>
    public class XmlUtil
    {
        #region 反序列化
        /// <summary>
        /// Xml反序列化成T
        /// </summary>
        /// <typeparam name="T">要反序列化成的类型</typeparam>
        /// <param name="xml">xml字符串</param>
        /// <returns></returns>
        public static T XmlDeserialize<T>(string xml) where T:class,new()
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(typeof(T));
                    return xmldes.Deserialize(sr) as T;
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }
       /// <summary>
        /// Xml Stream反序列化成T
       /// </summary>
        /// <typeparam name="T">要序列化成的类型</typeparam>
       /// <param name="stream">xml流</param>
       /// <returns></returns>
        public static T XmlDeserialize<T>(Stream stream) where T:class,new()
        {
            try
            {
                XmlSerializer xmldes = new XmlSerializer(typeof(T));
                return xmldes.Deserialize(stream) as T;
            }
            catch
            {
                return default(T);
            }
        }
        #endregion

        #region 序列化
        /// <summary>
        /// object序列化成Xml字符串
        /// </summary>
        /// <param name="t">要序列化成xml字符串的对象</param>
        /// <returns></returns>
        public static string XmlSerializer(object t)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(t.GetType());
            try
            {
                //序列化对象
                xml.Serialize(Stream, t);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();

            return str;
        }

        #endregion
    }
}
