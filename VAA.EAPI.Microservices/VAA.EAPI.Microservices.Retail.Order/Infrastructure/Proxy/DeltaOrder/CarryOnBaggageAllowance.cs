using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class CarryOnBaggageAllowance
    {
        public List<string> FlightNum { get; set; }
        public FreeBaggageAllowance FreeBaggageAllowance { get; set; }
    }
}
