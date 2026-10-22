using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCSeatUpdate
{
    public class SeatUpdateCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
