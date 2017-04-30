using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TrunkCommon
{
    public static class XMLHelper
    {
        /// <summary>
        /// 尝试获取XmlNode的属性值
        /// </summary>
        public static string TryGetXmlNodeAttributeValue(this XmlNode node, string attrName, string defVal)
        {
            if (node == null)
                return defVal;

            XmlAttribute attr = node.Attributes[attrName];
            return attr == null ? defVal : attr.Value;
        }
        /// <summary>
        /// 读取标准AppSettings配置文件
        /// </summary>
        /// <param name="configFileName"></param>
        /// <returns></returns>
        public static Configuration LoadConfiguration(string configFileName)
        {
            if (string.IsNullOrEmpty(configFileName))
                return null;
            ExeConfigurationFileMap file = new ExeConfigurationFileMap();
            file.ExeConfigFilename = configFileName;//配置文件路径
            return ConfigurationManager.OpenMappedExeConfiguration(file, ConfigurationUserLevel.None);
        }

        /// <summary>
        /// 在字符串中读取xml标记中的值
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string[] ReadTag(this string xml, string tag)
        {
            if (string.IsNullOrEmpty(xml) || string.IsNullOrEmpty(tag))
                return new string[] { string.Empty };

            string tagStart = string.Concat('<', tag, '>');
            string tagEnd = string.Concat("</", tag, '>');
            List<string> result = null;

            int index1 = -1, index2 = tagEnd.Length * -1;
            while ((index1 = xml.IndexOf(tagStart, index2 + tagEnd.Length)) != -1)
            {
                index2 = xml.IndexOf(tagEnd, index1 + tagStart.Length);
                if (index2 == -1)
                {
                    index2 = index1 + tagStart.Length - tagEnd.Length;
                    continue;
                }

                if (result == null)
                    result = new List<string>(4);

                result.Add(xml.Substring(index1 + tagStart.Length, index2 - index1 - tagStart.Length));
            }

            if (result == null)
                return new string[] { string.Empty };

            return result.ToArray();
        }

        /// <summary>
        /// 将object对象序列化成XML
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ToXML<T>(this T obj, Encoding encoding)
        {
            XmlSerializer ser = new XmlSerializer(obj.GetType());
            using (MemoryStream mem = new MemoryStream())
            {
                using (XmlTextWriter writer = new XmlTextWriter(mem, encoding))
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add(string.Empty, string.Empty);
                    ser.Serialize(writer, obj);
                    return encoding.GetString(mem.ToArray()).Trim();
                }
            }
        }

        /// <summary>
        /// 将XML反序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static T ParseXML<T>(this string source, Encoding encoding)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream(encoding.GetBytes(source)))
            {
                return (T)ser.Deserialize(stream);
            }
        }
    }//end class

    [Serializable]
    public class XmlConfig : IEnumerable<KeyValuePair<string, XmlConfigOptgroup>>, IEnumerable
    {
        #region 属性
        private Dictionary<string, XmlConfigOptgroup> Data { get; set; }
        private static readonly XmlConfigOptgroup EmptyXmlConfigOptgroup = new XmlConfigOptgroup();

        //private string _ConfigFilePath = null;
        ///// <summary>
        ///// XML配置文件的物理地址
        ///// </summary>
        //public string ConfigFilePath
        //{
        //    get { return _ConfigFilePath; }
        //    set { _ConfigFilePath = value; }
        //}
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        public XmlConfig(string configFile)
        {
            //this.ConfigFilePath = configFile;
            this.LoadXMLConfig(configFile);
        }
        public delegate void XmlConfigOptgroupEach(XmlConfigOptgroup group);
        /// <summary>
        /// 遍历该选项的所有组
        /// </summary>
        /// <param name="each"></param>
        public void Each(XmlConfigOptgroupEach each)
        {
            foreach (KeyValuePair<string, XmlConfigOptgroup> item in this.Data)
                each(item.Value);
        }
        /// <summary>
        /// 通过配置组ID获取或设置相应的配置组数据，不存在则返回null
        /// </summary>
        public XmlConfigOptgroup this[string optgroupID]
        {
            get { return this.Data.Get(optgroupID, null); }
            set { this.Data[optgroupID] = value; }
        }

        /// <summary>
        /// 通过配置组ID获取或设置相应的配置组数据，不存在则返回defValue
        /// </summary>
        /// <returns></returns>
        public XmlConfigOptgroup this[string optgroupID, XmlConfigOptgroup defValue]
        {
            get { return this.Data.Get(optgroupID, defValue); }
            set { this.Data[optgroupID] = value; }
        }

        /// <summary>
        /// 通过配置组ID获取或设置相应的配置组数据，不存在则返回defValue
        /// </summary>
        /// <returns></returns>
        public XmlConfigOptgroup this[string optgroupID, bool isNullReturnEmpty]
        {
            get { return this.Data.Get(optgroupID, isNullReturnEmpty ? EmptyXmlConfigOptgroup : null); }
            set { this.Data[optgroupID] = value; }
        }

        /// <summary>
        /// 获取默认配置组（第一个配置组）
        /// </summary>
        public XmlConfigOptgroup Default
        {
            get
            {
                foreach (KeyValuePair<string, XmlConfigOptgroup> item in this.Data)
                    return item.Value;
                return new XmlConfigOptgroup();
            }
        }
        /// <summary>
        /// 获取配置组个数
        /// </summary>
        public int Count
        {
            get { return this.Data.Count; }
        }
        /// <summary>
        /// 判断配置中是否存在该配置组
        /// </summary>
        /// <param name="optgroupID"></param>
        /// <returns></returns>
        public bool Contains(string optgroupID)
        {
            return this.Data.ContainsKey(optgroupID);
        }
        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadXMLConfig(string configFile)
        {
            this.Data = new Dictionary<string, XmlConfigOptgroup>();

            XmlDocument xml = new XmlDocument();
            xml.Load(configFile);

            XmlNodeList optgroups = xml.SelectNodes("/config/optgroup");
            XmlConfigOptgroup newXMLConfigOptgroup = null;
            XmlConfigOption newXMLConfigOption = null;
            XmlNodeList options = null;
            string tmpOptgroupID = null;
            string tmpOptionID = null;

            if (optgroups != null && optgroups.Count > 0)
            {
                for (int i = 0; i < optgroups.Count; i++)
                {
                    tmpOptgroupID = optgroups[i].TryGetXmlNodeAttributeValue("id", string.Concat("optgroup", i));
                    newXMLConfigOptgroup = new XmlConfigOptgroup();
                    for (int m = 0; m < optgroups[i].Attributes.Count; m++)
                        newXMLConfigOptgroup.Attr(optgroups[i].Attributes[m].Name, optgroups[i].Attributes[m].Value);

                    options = optgroups[i].SelectNodes("option"); //xml.SelectNodes(string.Concat("/config/optgroup[@id='", tmpOptgroupID, "']/option"));
                    if (options != null && options.Count > 0)
                    {
                        for (int j = 0; j < options.Count; j++)
                        {
                            newXMLConfigOption = new XmlConfigOption();
                            if (!string.IsNullOrEmpty(options[j].InnerText))
                                newXMLConfigOption["value"] = options[j].InnerText;
                            foreach (XmlAttribute optionAttribute in options[j].Attributes)
                                newXMLConfigOption[optionAttribute.Name] = optionAttribute.Value;
                            newXMLConfigOption.Value = newXMLConfigOption["value"];
                            tmpOptionID = options[j].TryGetXmlNodeAttributeValue("id", string.Concat("option", j));
                            newXMLConfigOptgroup[tmpOptionID] = newXMLConfigOption;
                        }
                    }
                    this.Data.Add(tmpOptgroupID, newXMLConfigOptgroup);
                }
            }
        }

        #region IEnumerable<KeyValuePair<string,XMLConfigOptgroup>> 成员

        public IEnumerator<KeyValuePair<string, XmlConfigOptgroup>> GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        #endregion

        #region IEnumerable 成员

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        #endregion
    }

    [Serializable]
    public class XmlConfigOptgroup : IEnumerable<KeyValuePair<string, XmlConfigOption>>, IEnumerable
    {
        private Dictionary<string, XmlConfigOption> Data = null;
        private Dictionary<string, string> Attribute = null;
        private static readonly XmlConfigOption EmptyXmlConfigOption = new XmlConfigOption();

        public string ID
        {
            get { return this.Attr("id"); }
        }

        public XmlConfigOptgroup()
        {
            this.Data = new Dictionary<string, XmlConfigOption>();
            this.Attribute = new Dictionary<string, string>();
        }
        /// <summary>
        /// 获取配置组的属性，没有则返回null
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public string Attr(string attributeName)
        {
            return this.Attribute.Get(attributeName, null);
        }

        /// <summary>
        /// 增加新属性
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Attr(string name, string value)
        {
            this.Attribute[name] = value;
        }

        /// <summary>
        /// 通过配置项ID获取后设置相应的配置项数据，没有则返回null
        /// </summary>
        public XmlConfigOption this[string optionID]
        {
            get { return this.Data.Get(optionID, null); }
            set { this.Data[optionID] = value; }
        }

        /// <summary>
        /// 通过配置项ID获取后设置相应的配置项数据，没有则返回defOption
        /// </summary>
        public XmlConfigOption this[string optionID, XmlConfigOption defOption]
        {
            get { return this.Data.Get(optionID, defOption); }
            set { this.Data[optionID] = value; }
        }

        public XmlConfigOption this[string optionID, bool isNullReturnEmpty]
        {
            get { return this.Data.Get(optionID, isNullReturnEmpty ? EmptyXmlConfigOption : null); }
            set { this.Data[optionID] = value; }
        }

        /// <summary>
        /// 通过配置项ID获取后设置相应的配置项数据，没有则返回null
        /// </summary>
        public XmlConfigOption Option(string optionID)
        {
            return this[optionID];
        }

        /// <summary>
        /// 通过配置项ID获取后设置相应的配置项数据，没有则返回defOption
        /// </summary>
        public XmlConfigOption Option(string optionID, XmlConfigOption defOption)
        {
            return this[optionID, defOption];
        }

        /// <summary>
        /// 通过选项属性和属性值查找返回第一个符合条件的选项，没有则返回null
        /// </summary>
        public XmlConfigOption Find(string attributeName, string attributeValue)
        {
            foreach (KeyValuePair<string, XmlConfigOption> item in this.Data)
                if (item.Value[attributeName] == attributeValue)
                    return item.Value;
            return null;
        }
        /// <summary>
        /// 通过选项属性和属性值查找返回所有符合条件的选项，不会返回null
        /// </summary>
        public List<XmlConfigOption> FindAll(string attributeName, string attributeValue)
        {
            List<XmlConfigOption> options = new List<XmlConfigOption>();
            foreach (KeyValuePair<string, XmlConfigOption> item in this.Data)
                if (item.Value[attributeName] == attributeValue)
                    options.Add(item.Value);
            return options;
        }

        /// <summary>
        /// 获取默认配置项（第一个配置项）
        /// </summary>
        public XmlConfigOption Default
        {
            get
            {
                foreach (KeyValuePair<string, XmlConfigOption> item in this.Data)
                    return item.Value;
                return new XmlConfigOption();
            }
        }

        /// <summary>
        /// 判断配置中是否存在该配置项
        /// </summary>
        /// <returns></returns>
        public bool Contains(string optionID)
        {
            return this.Data.ContainsKey(optionID);
        }

        public delegate void XmlConfigOptgroupEach(XmlConfigOption option);
        /// <summary>
        /// 遍历该配置组的所有option项
        /// </summary>
        /// <param name="each"></param>
        public void Each(XmlConfigOptgroupEach each)
        {
            foreach (KeyValuePair<string, XmlConfigOption> item in this.Data)
                each(item.Value);
        }
        /// <summary>
        /// 获取该配置组中包含的配置项个数
        /// </summary>
        public int Count
        {
            get { return this.Data.Count; }
        }

        public string ToJSON()
        {
            StringBuilder result = new StringBuilder(256);
            result.Append("{\"id\":\"").Append(this.ID).Append("\"");
            foreach (KeyValuePair<string, string> item in this.Attribute)
            {
                if (!"id".Equals(item.Key, StringComparison.CurrentCultureIgnoreCase))
                    result.Append(",\"").Append(item.Key).Append("\":\"").Append(item.Value).Append("\"");
            }
            result.Append(",\"options\":[");
            foreach (KeyValuePair<string, XmlConfigOption> item in this.Data)
            {
                result.Append(item.Value.ToJSON()).Append(",");
            }
            return result.ToString(0, result.Length - 1) + "]}";
        }

        #region IEnumerable<KeyValuePair<string,XMLConfigOption>> 成员

        public IEnumerator<KeyValuePair<string, XmlConfigOption>> GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        #endregion

        #region IEnumerable 成员

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        #endregion
    }

    [Serializable]
    public class XmlConfigOption : IEnumerable<KeyValuePair<string, string>>, IEnumerable
    {
        private Dictionary<string, string> Data = null;
        private string _Value = null;
        public string ID
        {
            get { return this.Attr("id"); }
        }

        /// <summary>
        /// 配置项值
        /// </summary>
        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }
        public XmlConfigOption()
        {
            this.Data = new Dictionary<string, string>();
        }

        public XmlConfigOption(string value, params KeyValuePair<string, string>[] attribute)
        {
            this.Value = value;
            foreach (KeyValuePair<string, string> item in attribute)
                this.Data.Add(item.Key, item.Value);
        }

        /// <summary>
        /// 获取或设置选项属性值，不存在则返回null
        /// </summary>
        /// <returns></returns>
        public string this[string attributeName]
        {
            get { return this.Data.Get(attributeName, null); }
            set { this.Data[attributeName] = value; }
        }

        /// <summary>
        /// 获取或设置选项属性值，不存在则返回defValue
        /// </summary>
        /// <returns></returns>
        public string this[string attributeName, string defValue]
        {
            get { return this.Data.Get(attributeName, defValue); }
            set { this.Data[attributeName] = value; }
        }
        /// <summary>
        /// 获取选项属性值，不存在则返回null
        /// </summary>
        public string Attr(string attributeName)
        {
            return this[attributeName];
        }

        /// <summary>
        /// 获取选项属性值，不存在则返回defValue
        /// </summary>
        public string Attr(string attributeName, string defValue)
        {
            return this[attributeName, defValue];
        }
        public delegate void XmlConfigOptionEach(KeyValuePair<string, string> kv);
        /// <summary>
        /// 遍历该选项的所有属性
        /// </summary>
        /// <param name="each"></param>
        public void Each(XmlConfigOptionEach each)
        {
            foreach (KeyValuePair<string, string> item in this.Data)
                each(item);
        }
        /// <summary>
        /// 获取该配置项包含的属性个数
        /// </summary>
        public int Count
        {
            get { return this.Data.Count; }
        }

        /// <summary>
        /// 判断配置项中是否存在某属性
        /// </summary>
        /// <returns></returns>
        public bool Contains(string attributeName)
        {
            return this.Data.ContainsKey(attributeName);
        }

        public string ToJSON()
        {
            string result = string.Concat("{\"id\":\"", this.ID, "\"");
            foreach (KeyValuePair<string, string> item in this.Data)
            {
                if (!"id".Equals(item.Key, StringComparison.CurrentCultureIgnoreCase))
                    result = string.Concat(result, ",\"", item.Key, "\":\"", item.Value, "\"");
            }
            return result + "}";
        }
        #region IEnumerable<KeyValuePair<string,string>> 成员

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        #endregion

        #region IEnumerable 成员

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        #endregion
    }

    public static class XmlConfigExtension
    {
        public static XmlConfigOptgroup Get(this XmlConfig xmlConfig, string optgroupID)
        {
            if (xmlConfig == null)
                return null;

            return xmlConfig[optgroupID];
        }
        public static XmlConfigOption Get(this XmlConfigOptgroup xmlConfigOptgroup, string optionID)
        {
            if (xmlConfigOptgroup == null)
                return null;

            return xmlConfigOptgroup[optionID];
        }
    }
}//end namespace
