using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerSearchCommands
{
    public class CustomerSearchUpdateCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}