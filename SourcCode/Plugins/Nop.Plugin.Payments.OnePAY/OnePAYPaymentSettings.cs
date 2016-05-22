using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.OnePAY
{
    public class OnePAYPaymentSettings : ISettings
    {
        public string Sandbox { get; set; }
        public bool UseSandbox { get; set; }
        public string MerchantID { get; set; }
        public string AccessCode { get; set; }
        public decimal AdditionalFee { get; set; }
        public string HashCode { get; set; }
    }
}
