using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.FlightInformation.Application.Command.Airportdetails
{
    public class AirportDetailsProcessCommand : IRequest<Result<Response<string>>>
    {

    }
}
