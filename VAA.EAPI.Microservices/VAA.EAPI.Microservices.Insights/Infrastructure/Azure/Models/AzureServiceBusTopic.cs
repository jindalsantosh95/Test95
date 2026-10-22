using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models
{
    public class AzureServiceBusTopic
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public AzureServiceBusEntityProperties Properties { get; set; }
        public IEnumerable<AzureServiceBusSubscription> Subscriptions { get; set; } = Enumerable.Empty<AzureServiceBusSubscription>();
    }
}
