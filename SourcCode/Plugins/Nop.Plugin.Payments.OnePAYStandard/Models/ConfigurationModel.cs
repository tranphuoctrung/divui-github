
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.OnePAYStandard.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.OnePAYStandard.Fields.Sandbox")]
        public string Sandbox { get; set; }

        [NopResourceDisplayName("Plugins.Payments.OnePAYStandard.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        [NopResourceDisplayName("Plugins.Payments.OnePAYStandard.Fields.MerchantID")]
        public string MerchantID { get; set; }

        [NopResourceDisplayName("Plugins.Payments.OnePAYStandard.Fields.AccessCode")]
        public string AccessCode { get; set; }

        [NopResourceDisplayName("Plugins.Payments.OnePAYStandard.Fields.HashCode")]
        public string HashCode { get; set; }

        [NopResourceDisplayName("Plugins.Payments.OnePAYStandard.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }       
    }
}