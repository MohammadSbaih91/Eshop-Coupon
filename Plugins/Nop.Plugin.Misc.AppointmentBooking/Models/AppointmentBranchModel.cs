using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AppointmentBooking.Models
{
    public class AppointmentBranchModel : BaseNopEntityModel, ILocalizedModel<AppointmentBranchLocalizedModel>
    {
        public string BranchId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.AppointmentBooking.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Misc.AppointmentBooking.Fields.Longitude")]
        public string Longitude { get; set; }

        [NopResourceDisplayName("Plugins.Misc.AppointmentBooking.Fields.Latitude")]
        public string Latitude { get; set; }

        public IList<AppointmentBranchLocalizedModel> Locales { get; set; }
    }

    public partial class AppointmentBranchLocalizedModel : ILocalizedLocaleModel
    {
        public AppointmentBranchLocalizedModel()
        {

        }

        public int LanguageId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.AppointmentBooking.Fields.Name")]
        public string Name { get; set; }
    }
}
