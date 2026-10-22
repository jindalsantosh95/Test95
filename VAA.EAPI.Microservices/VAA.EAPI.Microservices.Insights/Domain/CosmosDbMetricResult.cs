using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public class CosmosDbMetricResult
    {
        public string ServiceName { get; set; }

        public IEnumerable<CosmosDbMetricsData> CosmosDbMetrics { get; set; }
    }
}
