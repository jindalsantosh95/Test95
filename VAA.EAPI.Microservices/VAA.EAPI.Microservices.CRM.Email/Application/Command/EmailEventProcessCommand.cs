using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command
{
    public class EmailEventProcessCommand : CloudEvent, IRequest<bool>
    {

    }
}
