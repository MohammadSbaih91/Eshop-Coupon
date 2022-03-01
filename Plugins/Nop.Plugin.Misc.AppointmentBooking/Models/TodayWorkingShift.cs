using Newtonsoft.Json;

namespace Nop.Plugin.Misc.AppointmentBooking.Models
{
    public partial class TodayWorkingShift
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }
    }
}
