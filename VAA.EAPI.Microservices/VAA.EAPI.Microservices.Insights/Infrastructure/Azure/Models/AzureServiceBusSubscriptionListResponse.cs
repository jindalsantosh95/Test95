using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models
{
    public class AzureServiceBusSubscriptionListResponse
    {
        public IEnumerable<AzureServiceBusSubscription> Value { get; set; }
        public string NextLink { get; set; }
    }
}
