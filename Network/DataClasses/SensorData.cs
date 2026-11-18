using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SuperVisionObjectDesktop._3dParts.NetworkManager;

namespace SuperVisionObjectDesktop.Network.DataClasses
{
    public class SensorData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("matrix_rows")]
        public int MatrixRows { get; set; }

        [JsonProperty("matrix_cols")]
        public int MatrixCols { get; set; }

        [JsonProperty("data")]
        public SensorValues Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("matrix")]
        public List<List<uint>> Matrix { get; set; }
    }
}
