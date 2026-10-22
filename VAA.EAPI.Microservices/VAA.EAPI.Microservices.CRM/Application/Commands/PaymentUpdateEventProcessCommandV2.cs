using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.CRM.Application.Commands
{
    public class PaymentUpdateEventProcessCommandV2 : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
