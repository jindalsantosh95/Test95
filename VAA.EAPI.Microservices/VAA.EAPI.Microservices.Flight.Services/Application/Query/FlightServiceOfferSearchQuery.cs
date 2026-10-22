using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Flight.Services.Domain;

namespace VAA.EAPI.Microservices.Flight.Services.Application.Query
{
    public class FlightServiceOfferSearchQuery : IRequest<Result<Response<FlightServiceResponse>>>
    {
        public string FlightServiceOfferID { get; set; }

        public FlightServiceOfferSearchQuery(string flightServiceOfferID)
        {
            FlightServiceOfferID = flightServiceOfferID;
        }
    }
}
