using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.PaymentReference
{
    public class Fare
    {
        public string Fareclass { get; set; }
        public string Farebasis { get; set; }
        public IEnumerable<FareRestrictionByFlightSeg> FareRestrictionByFlightSeg { get; set; }
    }
}