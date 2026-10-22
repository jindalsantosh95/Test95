using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Events
{
    public class CreateTransactionEvent : CloudEvent, IRequest<Unit>
    {
    }
}