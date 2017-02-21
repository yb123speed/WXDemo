using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;



namespace Common
{
    /// <summary>
    /// 支付结果通知回调处理类
    /// 负责接收微信支付后台发送的支付结果并对订单有效性进行验证，将验证结果反馈给微信支付后台
    /// </summary>
    public class WxResultNotify:WxNotify
    {
        public System.IO.Stream _InputStream;
        public WxResultNotify() : base() { }
        public WxResultNotify(System.IO.Stream InputStream) { _InputStream = InputStream; }
        public new WxPayDataTool  ProcessNotify()
        {
            WxPayDataTool notifyData = GetNotifyData(_InputStream);

            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                WxPayDataTool res = new WxPayDataTool();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
               // Util.WriteFile(@"D:\ppp\log\wx.txt", "统一下单支付结果回调出错：" + res.ToXml());
                page.Response.Write(res.ToXml());
                page.Response.End();
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();
            string out_trade_no=notifyData.GetValue("out_trade_no").ToString();
            string total = notifyData.GetValue("total_fee").ToString();
            //查询订单，判断订单真实性
            OrderqueryInfo info = new OrderqueryInfo() { Transaction_id = transaction_id };
            if (!QueryOrder(info))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                WxPayDataTool res = new WxPayDataTool();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                //Util.WriteFile(@"D:\ppp\log\wx.txt", "订单查询失败：" + res.ToXml());
                page.Response.Write(res.ToXml());
                page.Response.End();
            }
            WxPayDataTool result = new WxPayDataTool();
            result.SetValue("transaction_id", transaction_id);
            result.SetValue("out_trade_no", out_trade_no);
            result.SetValue("total_fee", total);
            if (notifyData.IsSet("result_code"))
            result.SetValue("result_code", notifyData.GetValue("result_code").ToString());
            if (notifyData.IsSet("return_code"))
            result.SetValue("return_code", notifyData.GetValue("return_code").ToString());
            return result;
            
        }

        //查询订单
        private bool QueryOrder(OrderqueryInfo info)
        {
           // string logStr = @"D:\ppp\log\WXPay\wx_OrderQuery_" + DateTime.Today.ToString("yyyyMMdd") + ".txt";
            WxPayDataTool result = new WxPayDataTool();
            result = WxPayAction.Orderquery(info);

            if (!WxPayAction.CheckReturn(result))
            {
                //Util.WriteFile(logStr, @"订单查询信息失败=============" + DateTime.Now.ToString());
                //Util.WriteFile(logStr, result.ToJson());
                return false;
            }
            else
            {
                //Util.WriteFile(logStr, @"订单查询信息成功=============" + DateTime.Now.ToString());
                //Util.WriteFile(logStr, result.ToJson());
                return true;
            }
        }
       
    }
}