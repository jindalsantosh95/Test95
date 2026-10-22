using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM
{
    public class OTPEmailCommand : CloudEvent, IRequest<bool>
    {
    }
}
