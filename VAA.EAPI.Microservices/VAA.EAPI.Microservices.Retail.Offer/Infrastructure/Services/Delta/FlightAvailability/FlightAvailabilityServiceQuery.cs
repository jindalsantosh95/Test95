using MediatR;
using System;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlightAvailability;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightAvailability
{
    public class FlightAvailabilityServiceQuery : IRequest<Segment>
    {
        public string Carrier { get; set; }
        public int FlightNumber { get; set; }
        public DateTime FlightDate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string PointOfSale { get; set; }

        public FlightAvailabilityServiceQuery(FlightAvailabilityRepoQuery query)
        {
            Carrier = query.Carrier;
            FlightNumber = query.FlightNumber;
            FlightDate = query.FlightDate;
            Origin = query.Origin;
            Destination = query.Destination;
            PointOfSale = query.PointOfSale;
        }
    }
}
