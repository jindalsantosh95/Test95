using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription
{
    public class SubscriptionCreateCommand : SubscriptionCommandBase, IRequest<Result<Response<string>>>
    {
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public UserAccountDetails UserAccount { get; set; }
    }
}
