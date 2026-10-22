using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public class AppInsightsMetricsResult
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }
}
