using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Network.DataClasses
{
    public class TokenResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("expires")]
        public string Expires { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
