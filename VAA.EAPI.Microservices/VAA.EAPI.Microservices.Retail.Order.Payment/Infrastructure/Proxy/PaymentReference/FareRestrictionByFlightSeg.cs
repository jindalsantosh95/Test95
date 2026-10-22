using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.PaymentReference
{
    public class FareRestrictionByFlightSeg
    {
        public string OriginAirportCode { get; set; }
        public string DestAirportCode { get; set; }
        public IEnumerable<string> FareBasisCode { get; set; }
    }
}