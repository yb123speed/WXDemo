using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Common
{
    public class WxPayAction
    {
        //post数据
        public static WxPayDataTool GetResult(string url,WxPayDataTool inputobj,bool isUseCert=true)
        {
            string xml = inputobj.ToXml();
            //LogDB.DebugTest("发送的数据：" + xml);
            string response = UtilTool.Post(url, xml,isUseCert);
            //LogDB.DebugTest("接收的结果："+response);
            WxPayDataTool result = new WxPayDataTool();
            result.FromXml(response);
            return result;
        }

        //检测返回结果
        public static bool CheckReturn(WxPayDataTool result)
        {
            if (result != null)
            {
                bool flag1 = (result.IsSet("return_code") && result.GetValue("return_code").ToString().ToUpper() == "SUCCESS");
                bool flag2 = (result.IsSet("result_code") && result.GetValue("result_code").ToString().ToUpper() == "SUCCESS");
                return flag1 && flag2;
            }
            else
                return false;
        }

        //统一订单
        public static WxPayDataTool UnifiedOrder(UnifiedOrderInfo info)
        {   
            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            if (string.IsNullOrWhiteSpace(info.NonceStr)) info.NonceStr = UtilTool.GenerateCode(32);
            WxPayDataTool paytool = new WxPayDataTool();
            paytool.SetValue("appid", info.AppId);
            paytool.SetValue("mch_id", info.PartnerId);
            paytool.SetValue("device_info", "");
            paytool.SetValue("nonce_str", info.NonceStr);
            paytool.SetValue("body", info.Body);
            paytool.SetValue("attach", "");
            paytool.SetValue("out_trade_no", info.OutTradeNo);
            paytool.SetValue("total_fee", (int)info.TotalFee);
            paytool.SetValue("spbill_create_ip", info.SpbillCreateIp);
            paytool.SetValue("time_start","");
            paytool.SetValue("time_expire", "");
            paytool.SetValue("goods_tag", info.GoodsTag);
            paytool.SetValue("notify_url", info.NotifyUrl);
            paytool.SetValue("trade_type", info.Trade_type);
            paytool.SetValue("openid", info.OpenId);
            string signstr = paytool.MakeSign(info.PartnerKey);
            paytool.SetValue("sign", signstr);
            WxPayDataTool result = GetResult(url, paytool,false);

            if (!CheckReturn(result))
            {
                //LogDB.DebugTest("统一下单信息失败：" + result.ToJson());
            }
            return result;
        }

        //H5调起js支付
        public static WxPayDataTool GetJsApiParameters(UnifiedOrderInfo info)
        {
            string prepay_id = "";
            WxPayDataTool payresult = UnifiedOrder(info);
            if (CheckReturn(payresult))
             //   LogDB.DebugTest("统一下单信息出错：" + payresult.ToJson());
           // else
            {
                if (payresult.IsSet("prepay_id") && (prepay_id = payresult.GetValue("prepay_id").ToString()) != "")
                {
                    WxPayDataTool jsApiParam = new WxPayDataTool();
                    jsApiParam.SetValue("appId", info.AppId);
                    jsApiParam.SetValue("timeStamp", UtilTool.GenerateUnixTime());
                    jsApiParam.SetValue("nonceStr", UtilTool.GenerateCode(15));
                    jsApiParam.SetValue("package", "prepay_id=" + prepay_id);
                    jsApiParam.SetValue("signType", "MD5");
                    jsApiParam.SetValue("paySign", jsApiParam.MakeSign(info.PartnerKey));
                    return jsApiParam;
                }
            }
            return new WxPayDataTool();
        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static WxPayDataTool Orderquery(OrderqueryInfo info)
        {
            string url = "https://api.mch.weixin.qq.com/pay/orderquery";
            WxPayDataTool paytool = new WxPayDataTool();

            paytool.SetValue("appid", info.AppId);
            paytool.SetValue("mch_id", info.PartnerId);
            if (!string.IsNullOrEmpty(info.Transaction_id))
                paytool.SetValue("transaction_id", info.Transaction_id);
            if (!string.IsNullOrEmpty(info.Out_trade_no))
                paytool.SetValue("out_trade_no", info.Out_trade_no);
            paytool.SetValue("nonce_str", UtilTool.GenerateCode(32));//随机字符串    不超过32位
            paytool.SetValue("sign", paytool.MakeSign(info.PartnerKey));//签名
            WxPayDataTool result = GetResult(url, paytool, false);

            return result;
        }

        
    }
}
