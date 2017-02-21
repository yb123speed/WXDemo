using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Security.Cryptography;

namespace Common
{
    public class WxPayDataTool
    {
        public WxPayDataTool() { }

        private SortedDictionary<string, object> m_values = new SortedDictionary<string, object>();

        public void SetValue(string key, object value)
        {
            m_values[key] = value;

        }

        public object GetValue(string key)
        {
            object o = null;
            m_values.TryGetValue(key, out o);
            return o;
        }

        public bool IsSet(string key)
        {
            object o = null;
            m_values.TryGetValue(key, out o);
            return o != null;
        }

        /**
        * @将Dictionary转成xml
        * @return 经转换得到的xml串
        * @throws WxPayException
        **/
        public string ToXml()
        {   
            string temp="<xml></xml>";

            string xml = "<xml>";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                //字段值不能为null，会影响后续流程
                if(pair.Value!=null && pair.Value.ToString()!="")
                {
                    if (pair.Value.GetType() == typeof(int))
                    {
                        xml += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
                    }
                    else if (pair.Value.GetType() == typeof(string))
                    {
                        xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
                    }
                    else//除了string和int类型不能含有其他数据类型
                    {
                       
                        return temp;
                    }
                }
            }
            xml += "</xml>";
            return xml;
        }

        /**
        * @将xml转为WxPayData对象并返回对象内部的数据
        * @param string 待转换的xml串
        * @return 经转换得到的Dictionary
        * @throws WxPayException
        */
        public SortedDictionary<string, object> FromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return new SortedDictionary<string, object>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
            XmlNodeList nodes = xmlNode.ChildNodes;
            foreach (XmlNode xn in nodes)
            {
                XmlElement xe = (XmlElement)xn;
                m_values[xe.Name] = xe.InnerText;//获取xml的键值对到WxPayData内部的数据中
            }
            return m_values;
        }

        /**
        * @Dictionary格式转化成url参数格式
        * @ return url格式串, 该串不包含sign字段值
        */
        public string ToUrl(bool IscontainSign=false)
        {
            string buff = "";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                if (!IscontainSign)
                {
                    if (pair.Value != null && pair.Key != "sign" && pair.Value.ToString() != "")
                    {
                        buff += pair.Key + "=" + pair.Value + "&";
                    }
                }
                else
                {
                    if (pair.Value != null && pair.Value.ToString() != "")
                        buff += pair.Key + "=" + pair.Value + "&";
                }
            }
            buff = buff.Trim('&');
            return buff;
        }


        /**
        * @Dictionary格式化成Json
         * @return json串数据
        */
        public string ToJson()
        {
            string jsonStr = JsonConvert.SerializeObject(m_values);
            return jsonStr;
        }

    

        /**
        * @生成签名，详见签名生成算法
        * @return 签名, sign字段不参加签名
        */
        public string MakeSign(string key="")
        {
            //转url格式
            string str = ToUrl();
            //在string后加入API KEY
            str += "&key=" +key;
            //MD5加密
            var md5 = MD5.Create();
            var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            var sb = new StringBuilder();
            foreach (byte b in bs)
            {
                sb.Append(b.ToString("x2"));
            }
            //所有字符转为大写
            return sb.ToString().ToUpper();
        }



        /**
        * @获取Dictionary
        */
        public SortedDictionary<string, object> GetValues()
        {
            return m_values;
        }
    }
}
