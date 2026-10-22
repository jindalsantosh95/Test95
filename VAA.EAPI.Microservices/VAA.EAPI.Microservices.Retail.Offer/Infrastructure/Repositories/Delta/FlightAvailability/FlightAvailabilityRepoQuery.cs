using MediatR;
using System;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightAvailability;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlightAvailability
{
    public class FlightAvailabilityRepoQuery : IRequest<FlightAvailabilityResponse>
    {
        public string Carrier { get; set; }
        public int FlightNumber { get; set; }
        public DateTime FlightDate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string PointOfSale { get; set; }

        public FlightAvailabilityRepoQuery(FlightAvailabilityQuery query)
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
