using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public class DeploymentMetricsResult
    {
        public string ServiceName { get; set; }
        public DateTime LastDeployment { get; set; }
    }
}
