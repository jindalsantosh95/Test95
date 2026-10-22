using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models
{
    public class AzureSubscriptionsResponse
    {
        public IEnumerable<AzureSubscription> Value { get; set; } = new List<AzureSubscription>();
    }
}
