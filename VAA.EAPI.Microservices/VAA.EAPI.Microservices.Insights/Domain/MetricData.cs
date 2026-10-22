using System;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public class MetricData
    {
        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonPropertyName("average")]
        public double Average { get; set; }

        [JsonPropertyName("maximum")]
        public double Maximum { get; set; }
    }
}
