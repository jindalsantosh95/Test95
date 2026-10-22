using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class BaggageAllowance
    {
        public List<CheckedBaggageAllowance> CheckedBaggageAllowances { get; set; }
        public List<CarryOnBaggageAllowance> CarryOnBaggageAllowances { get; set; }
    }
}
