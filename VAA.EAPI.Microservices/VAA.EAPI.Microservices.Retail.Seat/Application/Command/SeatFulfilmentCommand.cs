using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Command
{
    public class SeatFulfilmentCommand : SeatFulfilmentRequest, IRequest<Result<Response<string>>>
    {
    }
}
