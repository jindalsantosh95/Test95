using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Command
{
    public class SeatSelectionUpdateCommand : SeatOfferUpdateRequest, IRequest<Result<Response<SeatAvailabilityResponse>>>
    {

    }
}
