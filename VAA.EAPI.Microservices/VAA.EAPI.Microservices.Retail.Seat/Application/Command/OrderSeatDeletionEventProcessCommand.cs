using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Command
{
    public class OrderSeatDeletionEventProcessCommand : CloudEvent , IRequest<Result<Unit>>
    {
    }
}
