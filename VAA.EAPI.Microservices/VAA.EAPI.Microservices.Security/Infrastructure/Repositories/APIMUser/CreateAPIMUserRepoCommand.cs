using MediatR;
using VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMUser
{
    public class CreateAPIMUserRepoCommand : IRequest<string>
    {
        public SubscriptionCreateCommand request { get; set; }

        public CreateAPIMUserRepoCommand(SubscriptionCreateCommand request)
        {
            this.request = request;
        }
    }
}
