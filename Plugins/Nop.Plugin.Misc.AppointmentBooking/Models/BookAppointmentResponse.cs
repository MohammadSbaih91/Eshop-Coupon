using Newtonsoft.Json;

namespace Nop.Plugin.Misc.AppointmentBooking.Models
{
    public partial class BookAppointmentResponse
    {
        public BookAppointmentResponse()
        {
            AppointmentTicketInfo = new AppointmentTicketInfo();
            TakeAppointmentResult = new TakeAppointmentResult();
        }

        [JsonProperty("appointmentTicketInfo")]
        public AppointmentTicketInfo AppointmentTicketInfo { get; set; }

        [JsonProperty("takeAppointmentResult")]
        public TakeAppointmentResult TakeAppointmentResult { get; set; }
    }

    public partial class AppointmentTicketInfo
    {
        public AppointmentTicketInfo()
        {
            Service = new Service();
        }

        [JsonProperty("appointmentDay")]
        public string AppointmentDay { get; set; }

        [JsonProperty("appointmentTime")]
        public string AppointmentTime { get; set; }

        [JsonProperty("service")]
        public Service Service { get; set; }

        [JsonProperty("ticketNumber")]
        public string TicketNumber { get; set; }
    }

    public partial class Service
    {
        [JsonProperty("availableForAppointment")]
        public bool AvailableForAppointment { get; set; }

        [JsonProperty("availableforWalkin")]
        public bool AvailableforWalkin { get; set; }

        [JsonProperty("departmentID")]
        public long DepartmentId { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("serviceGroupID")]
        public long ServiceGroupId { get; set; }
    }

    public partial class TakeAppointmentResult
    {
        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public partial class BookAppointmentResponse
    {
        public static BookAppointmentResponse FromJson(string json) => JsonConvert.DeserializeObject<BookAppointmentResponse>(json, Converter.Settings);
    }

    public static class BookAppointmentResponseSerialize
    {
        public static string ToJson(this BookAppointmentResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}
