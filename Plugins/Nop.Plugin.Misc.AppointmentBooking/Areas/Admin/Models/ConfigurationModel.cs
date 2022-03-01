using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AppointmentBooking.Areas.Admin.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Misc.AppointmentBooking.APIUrl")]
        public string APIUrl { get; set; }
        public bool APIUrl_OverrideForStore { get; set; }
    }
}
