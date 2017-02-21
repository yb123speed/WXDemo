namespace Common
{
    using System;
    using System.Runtime.CompilerServices;
    

    public class PayAccountInfo
    {
        public PayAccountInfo()
        {

            //部署时参数可以写在Web.Config里
            this.AppId = "wx88888888";      //微信的appId
            this.AppSecret = "";
            this.PartnerId = "88888888";           //微信里申请的商户号
            this.PartnerKey = "6532DC2EAF81691DB2F64145D1E081CF";   //微信支付设置的32位密钥
        }

        public PayAccountInfo(string appId, string appSecret, string partnerId, string partnerKey)
        {
            this.AppId = appId;
            this.AppSecret = appSecret;
            this.PartnerId = partnerId;
            this.PartnerKey = partnerKey;
        }

       

        public string AppId { get; set; }

        public string AppSecret { get; set; }

        public string PartnerId { get; set; }

        public string PartnerKey { get; set; }

    }
}

