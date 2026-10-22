using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class CarryOnBaggageAllowance
    {
        public IEnumerable<string> FlightNum { get; set; }
        public FreeBaggageAllowance FreeBaggageAllowance { get; set; }
        public string BaggageAllowanceId { get; set; }
    }
}