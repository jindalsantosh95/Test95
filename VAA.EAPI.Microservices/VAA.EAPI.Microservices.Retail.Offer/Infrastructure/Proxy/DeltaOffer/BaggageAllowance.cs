using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class BaggageAllowance
    {
        public IEnumerable<CheckedBaggageAllowance> CheckedBaggageAllowances { get; set; }
        public IEnumerable<CarryOnBaggageAllowance> CarryOnBaggageAllowances { get; set; }
    }
}
