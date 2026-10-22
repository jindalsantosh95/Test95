using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public class ServicePlanMetricResult
    {
        public string ServicePlanName { get; set; }
        public string ServicePlanFriendlyName { get; set; }
        public int CPUPercentageAverage { get; set; }
        public int CPUPercentageMax { get; set; }
        public int UsedMemoryPercentageAverage { get; set; }
        public int UsedMemoryPercentageMax { get; set; }
    }
}
