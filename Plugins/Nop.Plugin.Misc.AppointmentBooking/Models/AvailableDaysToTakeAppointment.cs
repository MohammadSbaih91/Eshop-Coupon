using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.AppointmentBooking.Models
{
    
    public partial class AvailableDaysToTakeAppointment
    {
        public AvailableDaysToTakeAppointment()
        {
            GetAvailableDaysToTakeAppointmentResult = new GetAvailableDaysToTakeAppointmentResult();
            ValidAppointmentDays = new List<string>();
        }

        [JsonProperty("getAvailableDaysToTakeAppointmentResult")]
        public GetAvailableDaysToTakeAppointmentResult GetAvailableDaysToTakeAppointmentResult { get; set; }

        [JsonProperty("validAppointmentDays")]
        public List<string> ValidAppointmentDays { get; set; }
    }

    public partial class GetAvailableDaysToTakeAppointmentResult
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }
    }

    public partial class AvailableDaysToTakeAppointment
    {
        public static AvailableDaysToTakeAppointment FromJson(string json) => JsonConvert.DeserializeObject<AvailableDaysToTakeAppointment>(json, Converter.Settings);
    }

    public static class AvailableDaysToTakeAppointmentSerialize
    {
        public static string ToJson(this AvailableDaysToTakeAppointment self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
    
}
