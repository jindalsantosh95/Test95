using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Domain.ADO
{
    public class ADOBuildTimelineResponse
    {
        [JsonProperty("records")]
        public IEnumerable<ADOBuildTimeline> BuildTimeLines { get; set; }
    }
}
