using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public class ServiceBusMetricsResult
    {
        public string ServiceBusName { get; set; }
        public int NumberOfMessagesInQueue { get; set; }
        public int NumberOfDeadLetterMessages { get; set; }
    }
}
