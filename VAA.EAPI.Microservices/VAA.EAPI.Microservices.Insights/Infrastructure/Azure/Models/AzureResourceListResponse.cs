using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models
{
    public class AzureResourceListResponse
    {
        public IEnumerable<AzureResource> Value { get; set; }
        public string NextLink { get; set; }

    }
}
