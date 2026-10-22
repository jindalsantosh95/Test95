using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability
{
    public class SeatAvailabilityQuery : SearchCriteria, IRequest<Result<Response<SeatAvailabilityResponse>>>
    {

    }
}
