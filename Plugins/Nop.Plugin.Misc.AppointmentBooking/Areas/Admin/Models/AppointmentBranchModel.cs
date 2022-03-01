using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AppointmentBooking.Areas.Admin.Models
{
    /// <summary>
    /// Represents a AppointmentBranchModel
    /// </summary>
    public partial class AppointmentBranchModel : BaseNopEntityModel, ILocalizedModel<AppointmentBranchLocalizedModel>
    {
        #region Ctor

        public AppointmentBranchModel()
        {
            Locales = new List<AppointmentBranchLocalizedModel>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Misc.AppointmentBooking.Fields.BranchId")]
        public string BranchId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.AppointmentBooking.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Misc.AppointmentBooking.Fields.Longitude")]
        public string Longitude { get; set; }

        [NopResourceDisplayName("Plugins.Misc.AppointmentBooking.Fields.Latitude")]
        public string Latitude { get; set; }

        public IList<AppointmentBranchLocalizedModel> Locales { get; set; }

        #endregion
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