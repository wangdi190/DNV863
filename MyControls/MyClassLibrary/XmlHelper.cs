using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyClassLibrary
{
    public static class XmlHelper
    {
        /// <summary>
        /// XML序列化
        /// </summary>
        /// <param name="xmlfilename">保存的xml文件全名</param>
        /// <param name="obj">将xml序列化的对象</param>
        public static void saveToXml(string xmlfilename, object obj)
        {
            if (string.IsNullOrWhiteSpace(xmlfilename)) return;

            string bakfilename = xmlfilename.Replace(".xml", ".bak");
            if (File.Exists(xmlfilename))
                File.Copy(xmlfilename, bakfilename, true);
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            Stream fs = new FileStream(xmlfilename, FileMode.Create);
            //XmlWriter writer = new XmlTextWriter(fs, Encoding.Unicode);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(fs, settings);

            serializer.Serialize(writer, obj);
            writer.Close();
            fs.Close();
        }

        /// <summary>
        /// xml反序列化
        /// </summary>
        /// <param name="xmlfilename">XML文件全名</param>
        /// <param name="type">反序列化后的对象类型</param>
        /// <returns></returns>
        public static object readFromXml(string xmlfilename, Type type)
        {
            object obj = null;

            if (xmlfilename.IndexOf("http:") < 0)
            {
                if (File.Exists(xmlfilename))
                {
                    XmlSerializer serializer = new XmlSerializer(type);

                    FileStream fs = new FileStream(xmlfilename, FileMode.Open);
                    XmlReader reader = XmlReader.Create(fs);
                    obj = serializer.Deserialize(reader);
                    reader.Close();
                    fs.Close();
                }
            }
            else
            {
                try
                {
                    XmlUrlResolver resolver = new XmlUrlResolver();
                    resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.XmlResolver = resolver;
                    XmlReader reader = XmlReader.Create(xmlfilename, settings);
                    XmlSerializer serializer = new XmlSerializer(type);
                    obj = serializer.Deserialize(reader);
                    reader.Close();
                }
                catch
                { }
            }
            return obj;
        }


        /// <summary>
        /// 二进制序列化
        /// </summary>
        /// <param name="datafilename">文件名</param>
        /// <param name="obj">要序列化的对象</param>
        public static void saveToDat(string datafilename,object obj)
        {

            FileStream fs = new FileStream(datafilename, FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, obj);
                fs.Close();
        }

        /// <summary>
        /// 二进制反序列化
        /// </summary>
        /// <param name="datafilename">文件名</param>
        /// <returns>返回对象</returns>
        public static object readFromDat(string datafilename)
        {
            object obj = null;
            if (File.Exists(datafilename))
            {
                FileStream fs = new FileStream(datafilename, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                obj = formatter.Deserialize(fs); 
                fs.Close();
            }
            return obj;
        }


    }
}
