
namespace Nop.Plugin.Misc.AppointmentBooking.Models
{
    public class ConfirmAppointment
    {
        public ConfirmAppointment()
        {
            BookAppointmentRequest = new BookAppointmentRequest();
            BookAppointmentResponse = new BookAppointmentResponse();
        }

        public BookAppointmentRequest BookAppointmentRequest { get; set; }

        public BookAppointmentResponse BookAppointmentResponse { get; set; }
    }
}
