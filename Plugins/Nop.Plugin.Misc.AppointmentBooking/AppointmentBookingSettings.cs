using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.AppointmentBooking
{
    public class AppointmentBookingSettings : ISettings
    {
        public string APIUrl { get; set; }

        public int PickUpInStoreId { get;set; }
    }
}
