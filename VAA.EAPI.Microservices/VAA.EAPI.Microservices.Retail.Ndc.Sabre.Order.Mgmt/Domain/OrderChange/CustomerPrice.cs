using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange
{
    public class CustomerPrice
    {
        public string Ptc { get; set; }
        [OpenApiProperty(Description = "The fare calculation for the customer type. This will probably go on the ticket. Only applicable for flight-service products")]
        public string FareCalc { get; set; }
        public IEnumerable<PriceComponent> PriceDetails { get; set; }
    }
}
