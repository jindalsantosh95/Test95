using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Flight.Services.Domain;

namespace VAA.EAPI.Microservices.Flight.Services.Application.Query
{
    public class FlightServiceQuery : FlightServiceRequest, IRequest<Result<Response<FlightServiceResponse>>>
    {
    }
}
