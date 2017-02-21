namespace Common
{
    using System;
    using System.Runtime.CompilerServices;

    public class UnifiedOrderInfo : PayAccountInfo
    {
        public UnifiedOrderInfo()
        {
            this.BankType = "WX";
            this.FeeType = "1";
            this.InputCharset = "UTF-8";
            this.SpbillCreateIp = "127.0.0.1";
            this.Trade_type = "APP";  //交易方式 NATIVE扫码支付 APP手机支付  JSAPI H5           

        }


        public string Attach { get; set; }

        public string BankType { get; private set; }

        public string Body { get; set; }

        public string FeeType { get; private set; }

        public string GoodsTag { get; set; }

        public string InputCharset { get; private set; }

        public string NotifyUrl { get; set; }

        public string OpenId { get; set; }

        public string OutTradeNo { get; set; }

        public string Partner { get; private set; }

        public decimal? ProductFee { get; set; }

        public string SpbillCreateIp { get; set; }

        public DateTime? TimeExpire { get; set; }

        public DateTime? TimeStart { get; set; }

        public decimal TotalFee { get; set; }

        public decimal? TransportFee { get; set; }

        //支付类型
        public string Trade_type { get; set; }


        public string NonceStr { get; set; }
    }
}

