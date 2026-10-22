using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Domain.ADO
{

    public class ADOPipelineResponse
    {
        [JsonProperty(PropertyName = "value")]
        public IEnumerable<ADOPipeline> Pipelines { get; set; }
    }
}
