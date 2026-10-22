using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;


namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands
{
    public class VoidTicketEventCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
