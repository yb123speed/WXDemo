using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using System.Web;


namespace Common
{
    public class UtilTool
    {
        /// <summary>
        /// 获取Unix时间戳
        /// </summary>
        /// <returns></returns>
        public static string GenerateUnixTime()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        //生成随机数字
        public static string RandomNum(int Length)
        {
            string str = "0123456789";
            string result = string.Empty;
            Random rd = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < Length; i++)
            {
                result += str[rd.Next(str.Length)];
            }
            return result;
        }

        //生成订单号
        public static string GenerateOutTradeNo()
        {
            string str = string.Empty;
            Random random = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < 7; i++)
            {
                int num = random.Next();
                str = str + ((char)(0x30 + ((ushort)(num % 10)))).ToString();
            }
            return (DateTime.Now.ToString("yyyyMMdd") + str);
        }

        //随机码  Noncestr
        public static string GenerateCode(int length)
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, length > 32 ? 32 : length);
        }

   

        public static string GetSignatureBySHA1(SortedDictionary<string, object> dic)
        {
            try
            {
                var string1Builder = new StringBuilder();
                foreach (KeyValuePair<string, object> pair in dic)
                {
                    if (pair.Value != null)
                    {
                        if (pair.Value.ToString() != "")
                            string1Builder.Append(pair.Key + "=" + pair.Value + "&");
                    }
                }
                string str = string1Builder.ToString();
                if (str != "")
                    str = str.Substring(0, str.Length - 1);
                string s = SHA1(str);
                return s;
            }
            catch (Exception err)
            {
               
                return "";
            }
        }

        public static string SHA1(string orgStr)
        {
            string strResult = "";
            System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create();
            byte[] bytResult = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(orgStr));
            for (int i = 0; i < bytResult.Length; i++)
            {
                strResult = strResult + bytResult[i].ToString("x2");
            }
            return strResult;
        }


        //格式化xml
        public static string FormatXml(string xmlstr)
        {
            if (xmlstr != null)
            {

                MemoryStream ms = null;
                XmlWriter xmlwt = null;

                try
                {
                    XmlWriterSettings setting = new XmlWriterSettings();

                    setting.OmitXmlDeclaration = true;//去除xml节点
                    setting.Indent = true;//是否缩进元素
                    Byte[] by = Encoding.UTF8.GetBytes(xmlstr);

                    ms = new MemoryStream(by);
                    xmlwt = XmlWriter.Create(ms, setting);

                    int count = (int)ms.Length;
                    byte[] temp = new byte[count];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(temp, 0, count);
                    UTF8Encoding ucode = new UTF8Encoding();
                    string returnValue = Encoding.UTF8.GetString(temp).Trim();
                    return returnValue;
                }
                catch (Exception ex) { return ""; }
                finally
                {
                    if (xmlwt != null)
                    {
                        xmlwt.Close();
                        ms.Close();
                        ms.Dispose();
                    }
                }

            }
            else
            { return ""; }
        }



        //支付签名
        public static string GetSignature(SortedDictionary<string, object> dic,string key="")
        {
            try
            {
                var string1Builder = new StringBuilder();
                foreach (KeyValuePair<string, object> pair in dic)
                {
                    string val = (pair.Value == null ? "" : pair.Value.ToString());
                    if (val.Trim() != "")
                        string1Builder.Append(pair.Key + "=" + val + "&");
                }
                string str = string1Builder.ToString();
                if (str != "")
                {
                    str = str.Substring(0, str.Length - 1);
                    str += "&key="+key;
                }

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
            catch (Exception err)
            {
                
                return "";
            }
        }


        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //直接确认，否则打不开    
            return true;
        }

        public static string Post(string url, string postdata,bool isUseCert=true)
        {
            System.GC.Collect();//垃圾回收，回收没有正常关闭的http连接
            
            string result = "";//返回结果
            int timeout = 10;
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream reqStream = null;

            try
            {
                //设置最大连接数
                ServicePointManager.DefaultConnectionLimit = 200;
                //设置https验证方式
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                /***************************************************************
                * 下面设置HttpWebRequest的相关属性
                * ************************************************************/
                request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "POST";
                request.Timeout = timeout * 1000;



                //设置POST的数据类型和长度
                request.ContentType = "text/xml";
                byte[] data = System.Text.Encoding.UTF8.GetBytes(postdata);
                request.ContentLength = data.Length;

                //是否使用证书

                if (isUseCert)
                {
                    string path = "";

                    if (HttpContext.Current != null)
                        path = HttpContext.Current.Request.PhysicalApplicationPath + "cert/apiclient_cert.p12";
                    else
                        path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cert/apiclient_cert.p12");


                    //X509Certificate2 cert = new X509Certificate2(path, set.WeixinPartnerID);
                    X509Certificate2 cert = new X509Certificate2(path, "1265257801", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable); 

                    request.ClientCertificates.Add(cert);
                }



                //往服务器写入数据
                reqStream = request.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();

                //获取服务端返回
                response = (HttpWebResponse)request.GetResponse();

                //获取服务端返回数据
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd().Trim();
                sr.Close();
            }
            catch (Exception err)
            {
               
                //System.Threading.Thread.ResetAbort();
            }
            finally
            {
                //关闭连接和流
                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
            }
            return result;
        }

        public static string Get(string url)
        {
            System.GC.Collect();
            string result = "";

            HttpWebRequest request = null;
            HttpWebResponse response = null;

            //请求url以获取数据
            try
            {
                //设置最大连接数
                ServicePointManager.DefaultConnectionLimit = 200;
                //设置https验证方式
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                /***************************************************************
                * 下面设置HttpWebRequest的相关属性
                * ************************************************************/
                request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "GET";


                //获取服务器返回
                response = (HttpWebResponse)request.GetResponse();

                //获取HTTP返回数据
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd().Trim();
                sr.Close();
            }
            catch (Exception e)
            {
                
            }
            finally
            {
                //关闭连接和流
                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
            }
            return result;
        }

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <returns></returns>
        public static string GetWebClientIp()
        {
            string userIP = "IP";

            try
            {
                if (System.Web.HttpContext.Current == null
            || System.Web.HttpContext.Current.Request == null
            || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";

                string CustomerIP = "";

                //CDN加速后取到的IP   
                CustomerIP = System.Web.HttpContext.Current.Request.Headers["Cdn-Src-Ip"];
                if (!string.IsNullOrEmpty(CustomerIP))
                {
                    return CustomerIP;
                }

                CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];


                if (!String.IsNullOrEmpty(CustomerIP))
                    return CustomerIP;

                if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (CustomerIP == null)
                        CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
                else
                {
                    CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

                }

                if (string.Compare(CustomerIP, "unknown", true) == 0)
                    return System.Web.HttpContext.Current.Request.UserHostAddress;
                return CustomerIP;
            }
            catch { }

            return userIP;
        }


    }
}
