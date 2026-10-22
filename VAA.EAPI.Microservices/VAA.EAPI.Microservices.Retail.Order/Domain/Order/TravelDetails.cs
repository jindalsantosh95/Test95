using System;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order
{
    public class TravelDetails
    {
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public Location Origin { get; set; }
        public Location Destination { get; set; }
    }
}