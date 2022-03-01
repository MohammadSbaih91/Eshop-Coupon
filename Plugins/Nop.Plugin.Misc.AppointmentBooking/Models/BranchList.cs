using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.AppointmentBooking.Models
{
    public partial class BranchList
    {
        public BranchList()
        {
            GetBranchesResult = new GetBranchesResult();
            Branches = new List<Branch>();
            AvailableAppointmentBranch = new List<SelectListItem>();
        }

        [JsonProperty("getBranchesResult")]
        public GetBranchesResult GetBranchesResult { get; set; }

        [JsonProperty("branches")]
        public List<Branch> Branches { get; set; }

        public List<SelectListItem> AvailableAppointmentBranch { get; set; }
        
    }
    
    public partial class GetBranchesResult
    {
        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }
    }

    public partial class BranchList
    {
        public static BranchList FromJson(string json) => JsonConvert.DeserializeObject<BranchList>(json, Converter.Settings);
    }

    public static class BranchListSerialize
    {
        public static string ToJson(this BranchList self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    
}
