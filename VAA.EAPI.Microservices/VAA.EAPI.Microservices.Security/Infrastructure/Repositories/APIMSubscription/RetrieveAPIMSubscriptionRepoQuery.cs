using MediatR;
using VAA.EAPI.Microservices.Security.Applications.Query.APIMSubscription;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription
{
    public class RetrieveAPIMSubscriptionRepoQuery : IRequest<SubscriptionResponse>
    {
        public SubscriptionRetrieveQuery Request { get; set; }
        public RetrieveAPIMSubscriptionRepoQuery(SubscriptionRetrieveQuery request)
        {
            Request = request;
        }
    }
}
