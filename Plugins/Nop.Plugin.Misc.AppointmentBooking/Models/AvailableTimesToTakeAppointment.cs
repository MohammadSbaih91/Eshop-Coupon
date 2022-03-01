using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.AppointmentBooking.Models
{
    public partial class AvailableTimesToTakeAppointment
    {
        public AvailableTimesToTakeAppointment()
        {
            GetDayAvailableTimesToTakeAppointmentResult = new GetDayAvailableTimesToTakeAppointmentResult();
            ValidDayAppointmentTimes = new List<string>();
        }

        [JsonProperty("getDayAvailableTimesToTakeAppointmentResult")]
        public GetDayAvailableTimesToTakeAppointmentResult GetDayAvailableTimesToTakeAppointmentResult { get; set; }

        [JsonProperty("validDayAppointmentTimes")]
        public List<string> ValidDayAppointmentTimes { get; set; }
    }

    public partial class GetDayAvailableTimesToTakeAppointmentResult
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }
    }

    public partial class AvailableTimesToTakeAppointment
    {
        public static AvailableTimesToTakeAppointment FromJson(string json) => JsonConvert.DeserializeObject<AvailableTimesToTakeAppointment>(json, Converter.Settings);
    }

    public static class AvailableTimesToTakeAppointmentSerialize
    {
        public static string ToJson(this AvailableTimesToTakeAppointment self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}

