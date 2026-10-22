using MediatR;
using VAA.EAPI.Microservices.Flight.Services.Domain;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Repositories
{
    public class FlightServiceOfferSearchRepoQuery : IRequest<FlightServiceResponse>
    {
        public string FlightServiceOfferID { get; set; }

        public FlightServiceOfferSearchRepoQuery(string flightServiceOfferID)
        {
            FlightServiceOfferID = flightServiceOfferID;
        }
    }
}
