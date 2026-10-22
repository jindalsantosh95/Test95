using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Domain.ADO
{
    internal class ADOPipelineRunsResponse
    {
        [JsonProperty("value")]
        public IEnumerable<ADOPipelineRun> PipelineRuns { get; set; }
    }
}
