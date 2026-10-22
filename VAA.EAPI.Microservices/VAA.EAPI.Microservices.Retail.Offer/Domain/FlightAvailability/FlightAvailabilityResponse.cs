using System;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightAvailability;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FlightAvailability
{

    public class FlightAvailabilityResponse
    {
        public FlightAvailabilityQuery Request { get; set; }
        public Segment Availability { get; set; }
        public DateTimeOffset Generated { get; set; }
    }

}
