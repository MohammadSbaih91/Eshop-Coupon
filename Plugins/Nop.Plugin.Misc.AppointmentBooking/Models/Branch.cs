using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.AppointmentBooking.Models
{
    public class Branch
    {
        public Branch()
        {
            TodayWorkingShifts = new List<TodayWorkingShift>();
        }
        
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("cityID")]
        public long CityId { get; set; }

        [JsonProperty("departmentID")]
        public long DepartmentId { get; set; }

        [JsonProperty("identity")]
        public string Identity { get; set; }

        [JsonProperty("isWorkingNow")]
        public bool IsWorkingNow { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("todayWorkingShifts")]
        public List<TodayWorkingShift> TodayWorkingShifts { get; set; }
    }
}
