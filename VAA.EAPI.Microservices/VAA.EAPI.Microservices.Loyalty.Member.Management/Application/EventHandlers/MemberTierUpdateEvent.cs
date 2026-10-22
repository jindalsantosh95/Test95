using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers
{
    public class MemberTierUpdateEvent : CloudEvent, IRequest<Result<Response<string>>>
    {
    }
}