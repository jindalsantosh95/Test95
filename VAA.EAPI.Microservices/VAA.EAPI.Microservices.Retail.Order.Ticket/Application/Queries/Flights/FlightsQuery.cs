using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Flights;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Queries.Flights
{
    public class FlightsQuery : IRequest<Result<Response<FlightResponse>>>
    {
        public FlightRequest FlightRequest { get; set; }

        public FlightsQuery(FlightRequest flightRequest) 
        { 
            FlightRequest = flightRequest;
        }
    }
}
