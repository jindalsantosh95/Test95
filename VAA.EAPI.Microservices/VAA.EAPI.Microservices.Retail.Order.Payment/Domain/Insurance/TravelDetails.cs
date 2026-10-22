using System;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance
{
    public class TravelDetails
    {
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public OriginDestinationDetails Origin { get; set; }
        public OriginDestinationDetails Destination { get; set; }
    }
}