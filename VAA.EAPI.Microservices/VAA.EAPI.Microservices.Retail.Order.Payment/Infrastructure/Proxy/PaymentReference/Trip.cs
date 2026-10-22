using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.PaymentReference
{
    public class Trip
    {
        public IEnumerable<FlightLeg> FlightLegs { get; set; }
        public int TripSeq { get; set; }
        public int TripTag { get; set; }
    }
}