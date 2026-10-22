using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerSearchCommands
{
    public class CustomerSearchAuditCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
