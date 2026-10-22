using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models
{
    public class AzureServiceBusNamespace : AzureResource
    {
        public IEnumerable<AzureServiceBusTopic> Topics { get; set; } = Enumerable.Empty<AzureServiceBusTopic>();
    }
}
