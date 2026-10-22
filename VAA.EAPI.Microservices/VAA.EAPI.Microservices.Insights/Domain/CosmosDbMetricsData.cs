using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public class CosmosDbMetricsData
    {
        public DateTime TimeStamp { get; set; }
        public int MaxNormalizedRUConsumptionPercentage { get; set; }
    }
}
