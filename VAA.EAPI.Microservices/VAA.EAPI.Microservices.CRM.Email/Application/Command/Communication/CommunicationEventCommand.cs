using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.Communication
{
    public class CommunicationEventCommand : CloudEvent, IRequest<bool>
    {
    }
}
