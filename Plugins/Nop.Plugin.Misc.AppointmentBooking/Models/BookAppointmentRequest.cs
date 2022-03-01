namespace Nop.Plugin.Misc.AppointmentBooking.Models
{
    public class BookAppointmentRequest
    {
        public int OrderId { get; set; }
        public int AddressId { get; set; }
        public string PhoneNumber { get; set; }

        public string AppointmentDay { get; set; }

        public string SelectedAppointmentTime { get; set; }

        public string BranchID { get; set; }

        public string ServiceID { get; set; }

        public string x_wassup_msisdn { get; set; }

        public string SelectedStoreName { get; set; }
    }
}
