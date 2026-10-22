using MediatR;
using VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription
{
    public class CreateAPIMSubscriptionRepoCommand : IRequest<SubscriptionResponse>
    {
        public SubscriptionCreateCommand Request { get; set; }
        public CreateAPIMSubscriptionRepoCommand(SubscriptionCreateCommand request)
        {
            Request = request;
        }
    }
}
