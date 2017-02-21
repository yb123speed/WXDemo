using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.IO;


namespace Common
{
    /// <summary>
    /// 回调处理基类
    /// 主要负责接收微信支付后台发送过来的数据，对数据进行签名验证
    /// 子类在此类基础上进行派生并重写自己的回调处理过程
    /// </summary>
    public class WxNotify
    {
        public Page page { get; set; }
        public WxNotify()
        {
           
        }

        /// <summary>
        /// 接收从微信支付后台发送过来的数据并验证签名
        /// </summary>
        /// <returns>微信支付后台返回的数据</returns>
        public WxPayDataTool GetNotifyData(Stream InputStream)
        {
            //接收从微信后台POST过来的数据
            System.IO.Stream s = InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();

            //转换数据格式并验证签名
            WxPayDataTool data = new WxPayDataTool();
            try
            {
                //LogDB.DebugTest("支付结果回调信息：" + builder.ToString());
                data.FromXml(builder.ToString());
            }
            catch (Exception ex)
            {
                //若签名错误，则立即返回结果给微信支付后台
                WxPayDataTool res = new WxPayDataTool();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                //LogDB.DebugTest("异步回调信息出错：" + res.ToXml());
                page.Response.Write(res.ToXml());
                page.Response.End();
            }
            return data;
        }



        //派生类需要重写这个方法，进行不同的回调处理
        public virtual WxPayDataTool ProcessNotify()
        {
            //1.微信公众号回调
            //2.扫码模式一与模式二
            return new WxPayDataTool();
        }
    }
}