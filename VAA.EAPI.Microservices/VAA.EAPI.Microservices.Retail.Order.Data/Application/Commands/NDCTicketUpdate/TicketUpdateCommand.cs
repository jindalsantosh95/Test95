using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCTicketUpdate
{
    public class TicketUpdateCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
