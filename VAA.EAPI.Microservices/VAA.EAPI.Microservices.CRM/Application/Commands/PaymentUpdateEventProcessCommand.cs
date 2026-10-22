using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.CRM.Application.Commands
{
    public class PaymentUpdateEventProcessCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
