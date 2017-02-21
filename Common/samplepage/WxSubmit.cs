namespace Common
{

  

    using Newtonsoft.Json;
    using System;
    using System.Web.UI;
    
    using System.Web;

    using System.Web.UI.WebControls;

    public class WxSubmit : Page
    {
        public string pay_json = string.Empty;
        string orderid = "";
        decimal paymoney = 0.01M;
        protected Label lblopenid;
        protected void Page_Load(object sender, EventArgs e)
        {
            //string str = base.Request.QueryString.Get("orderId");
            if (!IsPostBack)
            {
               
                string body="",orderid="",openid="";
                decimal paymoney=0;
                string NotifyUrl = "http://tianzhi.0519see.com/WeiPay/Call";
                this.pay_json=PayHelper.WxJsApiPay(body, NotifyUrl, orderid, openid, paymoney);
            }
                 
            
        }
    }
}

