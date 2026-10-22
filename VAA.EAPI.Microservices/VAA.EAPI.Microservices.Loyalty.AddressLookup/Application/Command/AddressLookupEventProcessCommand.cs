using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Command
{
    public class AddressLookupEventProcessCommand : CloudEvent, IRequest<Result<Unit>>
    {

    }
}
