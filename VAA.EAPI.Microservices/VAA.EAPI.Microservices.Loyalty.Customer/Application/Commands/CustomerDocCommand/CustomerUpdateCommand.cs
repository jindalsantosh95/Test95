using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand
{
    public class CustomerUpdateCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}