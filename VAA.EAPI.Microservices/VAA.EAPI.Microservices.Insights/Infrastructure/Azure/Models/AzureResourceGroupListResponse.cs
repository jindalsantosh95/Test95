using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models
{
    public class AzureResourceGroupListResponse
    {
        public IEnumerable<AzureResourceGroup> Value { get; set; }
        public string NextLink { get; set; }
    }
}
