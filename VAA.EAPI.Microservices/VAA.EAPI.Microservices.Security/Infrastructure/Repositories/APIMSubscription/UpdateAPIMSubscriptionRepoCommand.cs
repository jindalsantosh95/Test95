using MediatR;
using VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription
{
    public class UpdateAPIMSubscriptionRepoCommand : IRequest<SubscriptionResponse>
    {
        public SubscriptionUpdateCommand Request { get; set; }
        public UpdateAPIMSubscriptionRepoCommand(SubscriptionUpdateCommand request)
        {
            Request = request;
        }
    }
}
