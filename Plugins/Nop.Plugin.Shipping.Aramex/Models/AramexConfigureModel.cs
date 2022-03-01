using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.Aramex.Models
{
    public class AramexConfigureModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Shipping.Aramex.Configure.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Shipping.Aramex.Configure.UserName")]
        public string UserName { get; set; }
        public bool UserName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Shipping.Aramex.Configure.Password")]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }

        [NopResourceDisplayName("Shipping.Aramex.Configure.Version")]
        public string Version { get; set; }
        public bool Version_OverrideForStore { get; set; }

        [NopResourceDisplayName("Shipping.Aramex.Configure.AccountNumber")]
        public string AccountNumber { get; set; }
        public bool AccountNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Shipping.Aramex.Configure.AccountPin")]
        public string AccountPin { get; set; }
        public bool AccountPin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Shipping.Aramex.Configure.AccountEntity")]
        public string AccountEntity { get; set; }
        public bool AccountEntity_OverrideForStore { get; set; }

        [NopResourceDisplayName("Shipping.Aramex.Configure.AccountCountryCode")]
        public string AccountCountryCode { get; set; }
        public bool AccountCountryCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Shipping.Aramex.Configure.Source")]
        public string Source { get; set; }
        public bool Source_OverrideForStore { get; set; }

    }
}
