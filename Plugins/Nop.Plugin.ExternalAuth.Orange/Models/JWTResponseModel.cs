using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.ExternalAuth.Orange.Models
{
    public partial class JWTResponseModel
    {
        [JsonProperty("FName")]
        public string FName { get; set; }

        [JsonProperty("LName")]
        public string LName { get; set; }

        [JsonProperty("ContactNumber")]
        public string ContactNumber { get; set; }

        [JsonProperty("AlternativEmail")]
        public string AlternativEmail { get; set; }

        [JsonProperty("GUID")]
        public Guid Guid { get; set; }

        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonProperty("nbf")]
        public long Nbf { get; set; }

        [JsonProperty("exp")]
        public long Exp { get; set; }

        [JsonProperty("iat")]
        public long Iat { get; set; }

        [JsonProperty("iss")]
        public string Iss { get; set; }

        [JsonProperty("aud")]
        public string Aud { get; set; }
    }
}
