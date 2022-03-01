using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.eShopSMS.Areas.Admin.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Misc.eShopSMS.APIUrl")]
        public string APIUrl { get; set; }
        public bool APIUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.eShopSMS.UserName")]
        public string UserName { get; set; }
        public bool UserName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.eShopSMS.Password")]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }
    }
}
