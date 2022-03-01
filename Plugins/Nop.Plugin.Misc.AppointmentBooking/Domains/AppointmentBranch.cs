using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Misc.AppointmentBooking.Domains
{
    public partial class AppointmentBranch : BaseEntity, ILocalizedEntity
    {
        public string BranchId { get; set; }

        public string Name { get; set; }

        public string Longitude { get; set; }

        public string Latitude { get; set; }
    }
}
