using System.Collections.Generic;
using VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure
{
    public class AzureKeyVaultReferencesResponse
    {
        public IEnumerable<AzureKeyVaultReference> Value { get; set; }

        public string NextLink { get; set; }
    }
}
