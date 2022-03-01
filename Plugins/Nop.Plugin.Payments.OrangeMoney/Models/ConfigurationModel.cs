using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.OrangeMoney.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.OrangeMoney.Fields.MerchantCode")]
        public string MerchantCode { get; set; }
        public bool MerchantCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.OrangeMoney.Fields.ChannelCode")]
        public string ChannelCode { get; set; }
        public bool ChannelCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.OrangeMoney.Fields.APIEndPoint")]
        public string APIEndPoint { get; set; }

        [NopResourceDisplayName("Plugins.Payments.OrangeMoney.Fields.CallBackURL")]
        public string CallBackURL { get; set; }

        [NopResourceDisplayName("Plugins.Payments.OrangeMoney.Fields.PSP")]
        public string PSP { get; set; }
        public bool PSP_OverrideForStore { get; set; }
    }
}