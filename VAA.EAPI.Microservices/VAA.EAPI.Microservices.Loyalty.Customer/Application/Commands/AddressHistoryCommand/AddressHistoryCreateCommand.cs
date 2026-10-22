using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.AddressHistoryCommand
{
    public class AddressHistoryCreateCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }

}
