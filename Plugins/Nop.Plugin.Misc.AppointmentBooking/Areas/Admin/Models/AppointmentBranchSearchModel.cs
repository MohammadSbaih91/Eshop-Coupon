using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AppointmentBooking.Areas.Admin.Models
{
    public class AppointmentBranchSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Plugins.Misc.AppointmentBooking.Fields.Name")]
        public string Name { get; set; }
    }
}
