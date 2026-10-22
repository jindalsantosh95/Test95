using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.AddressUpdate
{
    public class AddressChangeEventCommand : CloudEvent, IRequest<Result<Response<string>>>
    {

    }
}
