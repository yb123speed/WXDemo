using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;


namespace Common
{
    public class PayHelper
    {

        //微信支付(公众号支付)
        public static string WxJsApiPay(string body, string NotifyUrl, string orderid, string openid, decimal paymoney)
        {
            UnifiedOrderInfo info = new UnifiedOrderInfo()
            {
                Body = body,
                OutTradeNo = orderid,
                OpenId = openid,
                TotalFee = (int)(Math.Round(paymoney, 2, MidpointRounding.AwayFromZero) * 100)
            };

            info.NotifyUrl = NotifyUrl;

            WxPayDataTool paytool = WxPayAction.GetJsApiParameters(info);
            string pay_json = paytool.ToJson();
            return pay_json;
        }

      

        //微信(返回二维码链接扫码付)===原生支付
        public static string GetQRCode(string body, string NotifyUrl, string orderid, string openid, decimal paymoney)
        {   
            UnifiedOrderInfo info = new UnifiedOrderInfo()
            {
                Body = body,       
                OutTradeNo = orderid,
                OpenId ="",
                TotalFee = (int)(Math.Round(paymoney, 2, MidpointRounding.AwayFromZero) * 100),
                NotifyUrl=NotifyUrl,
                Trade_type = "NATIVE"
            };
            WxPayDataTool paytool = WxPayAction.UnifiedOrder(info);
            bool flag = WxPayAction.CheckReturn(paytool);
            if (flag)
                return paytool.GetValue("code_url").ToString();//获得统一下单接口返回的二维码链接
            else
                return "";
        }


        public static SortedDictionary<string,object> AppPay(string body, string NotifyUrl, string orderid, string openid, decimal paymoney,string clientIp)
        {
            //string logStr=@"D:\ppp\log\WXPay\wx_AppPay_" + DateTime.Today.ToString("yyyyMMdd") + ".txt";
            UnifiedOrderInfo info = new UnifiedOrderInfo()
            {
                Body = body,
                OutTradeNo = orderid,
                OpenId = "",
                TotalFee = (int)(Math.Round(paymoney, 2, MidpointRounding.AwayFromZero) * 100),
                SpbillCreateIp=clientIp,
                NotifyUrl = NotifyUrl,
                Trade_type = "APP"
            };
            string NonceStr = UtilTool.GenerateCode(32);
            info.NonceStr = NonceStr;
            //Util.WriteFile(logStr, @"WX AppPay Call======================" + DateTime.Now.ToString());
            //Util.WriteFile(logStr, @"Client IP:" + info.SpbillCreateIp);
            WxPayDataTool paytool = WxPayAction.UnifiedOrder(info);
           // Util.WriteFile(logStr,@"微信返回信息:"+paytool.ToJson());
            if (paytool.GetValue("return_code").ToString() == "SUCCESS")
            {
                if (paytool.GetValue("result_code").ToString() == "SUCCESS")
                {
                    WxPayDataTool paytool2Sign = new WxPayDataTool();
                    PayAccountInfo payaccount = new PayAccountInfo();
                    paytool2Sign.SetValue("appid", payaccount.AppId);
                    paytool2Sign.SetValue("noncestr", info.NonceStr);//此参数虽然是随机码  但是必须和之前请求微信时的随机码一致
                    paytool2Sign.SetValue("package", "Sign=WXPay");
                    paytool2Sign.SetValue("partnerid", payaccount.PartnerId);
                    paytool2Sign.SetValue("prepayid", paytool.GetValue("prepay_id"));
                    paytool2Sign.SetValue("timestamp", UtilTool.GenerateUnixTime());


                    string sign = paytool2Sign.MakeSign(payaccount.PartnerKey);

                    paytool2Sign.SetValue("sign", sign);

                    return paytool2Sign.GetValues();
                }
                else
                    return null;
            }
            else
                return null;

        }
        


        //检测返回结果
        public static bool CheckReturn(WxPayDataTool result)
        {
            return WxPayAction.CheckReturn(result);
        }
 
    }
}
