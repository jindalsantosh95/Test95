using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Security.Applications.Events.APIMSubscription
{
    public class SubscriptionDeleteEventCommand : CloudEvent, IRequest<Result<Response<string>>>
    {
    }
}
