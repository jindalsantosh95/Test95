using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightAvailability;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightAvailability
{
    public class FlightAvailabilityQuery : IRequest<Result<Response<FlightAvailabilityResponse>>>
    {
        public string Carrier { get; set; }
        public int FlightNumber { get; set; }
        public DateTime FlightDate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string PointOfSale { get; set; }
    }
}
