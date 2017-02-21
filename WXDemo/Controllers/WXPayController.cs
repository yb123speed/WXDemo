using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using Common;
using System.Web.UI;
using ThoughtWorks.QRCode.Codec;

namespace WXDemo.Controllers
{
    //此处demo不做权限验证，项目发布时自己做好权限验证
    public class WXPayController : Controller
    {

        /// <summary>
        /// APP支付
        /// </summary>
        /// <param name="cash"></param>
        /// <returns></returns>
        public JsonResult AppPay(decimal cash)    
        {

            string body = "订单描述";
            string notifyurl = "http://localhost:4040/WXPay/WXCall";    //微信后台通知的地址
            string Recordid = UtilTool.GenerateOutTradeNo();
            string clientIp = UtilTool.GetWebClientIp();           //本地测试时地址为::1会导致微信返回错误   要在服务器上部署  远程调用调试
            SortedDictionary<string, object> wxRes = PayHelper.AppPay(body, notifyurl, Recordid, "", cash, clientIp);
            if (wxRes != null)
            {
               
                Dictionary<string, object> data = new Dictionary<string, object>();
                foreach (var item in wxRes)                 //微信APP支付需要的参数
                {
                    data.Add(item.Key, item.Value);
                }

                data.Add("orderid", Recordid);        //如果APP要需要网站订单号则返回网站订单号
                data.Add("code", "SUCCESS");
                return Json(data);
            }
            else
            {

                Dictionary<string, object> data = new Dictionary<string, object>();
                data.Add("code", "FAILD");        
                return Json(data);
            }

        }

        //二维码支付(NATIVE)
        [HttpGet]
        [OutputCache(Location = OutputCacheLocation.None, NoStore = false)]   //清除缓存
        public ActionResult QRCodePay(decimal cash)
        {
                string body = "账户充值";
                string Recordid = UtilTool.GenerateOutTradeNo();
                string notifyurl = "http://localhost:4040/WXPay/WXCall";      //自行更换域名
                string wxRes = PayHelper.GetQRCode(body, notifyurl, Recordid, "", cash);

                string pic = GetDimensionalImg(wxRes,Recordid );

                string html = @"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <title>Title</title>
    <style>
      *{
        margin: 0;
        padding: 0;
      }
      .layout-qrcode{
        width:100%;
      }
      .layout-qrcode .container{
        width:251px;
        margin:0 auto;
        border:1px solid #ddd;
        border-radius: 10px;
      }
      .layout-qrcode .content{
          padding:35px;
      }
      .layout-qrcode .qr-img{
        width:181px;
        height:181px;
      }
      .layout-qrcode .qr-txt{
          width:181px;
          margin: 15px auto 0;
      }
      .layout-qrcode .qr-txt img{
          width: 100%;
      }
    </style>
</head>
<body>
<div class='layout-qrcode'>
  <div class='container'>
    <div class='content'>
      <div class='qr-img'>
        <img src='" + pic + @"' height='181' width='181'/>
      </div>
      <div class='qr-txt'>
          <img src='/Content/wx/说明文字.png' alt=''>
      </div>
    </div>
  </div>
</div>
</body>
</html>";


            return Content(html);
        }


        /// <summary>
        /// 微信回调
        /// </summary>
        public void WXCall()
        {

            WxResultNotify resultNotify = new WxResultNotify(Request.InputStream);
            //string logStr = @"D:\ppp\log\WXPay\wx_callBack_" + DateTime.Today.ToString("yyyyMMdd") + ".txt";
            WxPayDataTool result = resultNotify.ProcessNotify();
            //Util.WriteFile(logStr, "WXPay CallBack---------------" + DateTime.Now.ToString());
            WxPayDataTool resturn = new WxPayDataTool();
            //Util.WriteFile(logStr, @"return_code:" + result.GetValue("return_code").ToString());
            //Util.WriteFile(logStr, @"result_code:" + result.GetValue("result_code").ToString());
            if (WxPayAction.CheckReturn(result))
            {
                //此处处理网站订单业务


                resturn.SetValue("return_code", "SUCCESS");
                resturn.SetValue("return_msg", "OK");


            }
            else
            {
                resturn.SetValue("return_code", "FAIL");
                resturn.SetValue("return_msg", "微信支付返回结果标识错误");

            }
            Response.Write(resturn.ToXml());
            Response.End();
        }

        /// <summary>
        /// 生成二维码并输出图片路径
        /// </summary>
        /// <param name="link"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private string GetDimensionalImg(string link, string id)
        {
            string src = "/QRCode/" + id + ".png";
            if (System.IO.File.Exists(Server.MapPath(src)))
                System.IO.File.Delete(Server.MapPath(src));

            System.Drawing.Image bmp = null;
            try
            {
                QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                qrCodeEncoder.QRCodeScale = 4;
                //int version = Convert.ToInt16(cboVersion.Text);
                qrCodeEncoder.QRCodeVersion = 7;
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
                bmp = qrCodeEncoder.Encode(link);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Invalid version !");
            }
            bmp.Save(Server.MapPath(src));


            return src;
        }
    }
}
