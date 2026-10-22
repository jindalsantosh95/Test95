using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.AddressUpdate
{
    public class AddressUpdateNotificationCommand : CloudEvent, IRequest<Result<Response<string>>>
    {

    }
}
