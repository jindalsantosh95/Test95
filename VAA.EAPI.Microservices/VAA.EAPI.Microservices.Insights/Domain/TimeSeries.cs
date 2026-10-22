using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public class TimeSeries
    {
        [JsonPropertyName("metadatavalues")]
        public object[] MetadataValues { get; set; }

        [JsonPropertyName("data")]
        public MetricData[] Data { get; set; }
    }
}
