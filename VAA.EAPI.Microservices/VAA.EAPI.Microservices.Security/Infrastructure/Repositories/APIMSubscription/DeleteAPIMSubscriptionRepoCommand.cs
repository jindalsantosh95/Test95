using MediatR;
using VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription
{
    public class DeleteAPIMSubscriptionRepoCommand : IRequest<SubscriptionResponse>
    {
        public string Uri { get; set; }
        public SubscriptionDeleteCommand Request { get; set; }
        public DeleteAPIMSubscriptionRepoCommand(string uri, SubscriptionDeleteCommand request)
        {
            Uri = uri;
            Request = request;
        }

    }
}
