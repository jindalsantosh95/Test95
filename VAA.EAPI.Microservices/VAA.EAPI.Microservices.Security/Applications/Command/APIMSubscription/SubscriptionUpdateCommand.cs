using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription
{
    public class SubscriptionUpdateCommand : SubscriptionCommandBase, IRequest<Result<Response<string>>>
    {
        public string SubscriptionId { get; set; }
        public string Enabled { get; set; }
    }
}
