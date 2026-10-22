using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public class ServicePlanMetricValue
    {
        public int UsedMemoryPercentage { get; set; }
        public int CPUPercentage { get; set; }
    }
}
