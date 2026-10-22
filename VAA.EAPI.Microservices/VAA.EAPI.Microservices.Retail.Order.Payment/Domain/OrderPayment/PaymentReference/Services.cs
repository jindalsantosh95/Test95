using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference
{
    public class Service
    {
        public IEnumerable<string> JourneyRefIds { get; set; }
        public IEnumerable<string> FlightSegmentsRefIds { get; set; }
        public FareDetail FareDetails { get; set; }
    }
}

