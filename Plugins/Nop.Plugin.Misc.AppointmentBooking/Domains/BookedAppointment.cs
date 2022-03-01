using Nop.Core;
using System;

namespace Nop.Plugin.Misc.AppointmentBooking.Domains
{
    public partial class BookedAppointment : BaseEntity
    {
        public int OrderId { get; set; }

        public string AppointmentDay { get; set; }

        public string AppointmentTime { get; set; }

        public string BranchID { get; set; }

        public string ServiceID { get; set; }

        public string x_wassup_msisdn { get; set; }

        public string TicketNumber { get; set; }

        public string JsonResponce { get; set; }

        public DateTime CreatedOnUTC { get; set; }

        public string SelectedStoreName { get; set; }
    }
}
