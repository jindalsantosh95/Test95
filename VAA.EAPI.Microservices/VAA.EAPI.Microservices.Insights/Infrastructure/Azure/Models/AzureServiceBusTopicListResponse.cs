using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models
{
    public class AzureServiceBusTopicListResponse
    {
        public IEnumerable<AzureServiceBusTopic> Value { get; set; }
        public string NextLink { get; set; }
    }
}
