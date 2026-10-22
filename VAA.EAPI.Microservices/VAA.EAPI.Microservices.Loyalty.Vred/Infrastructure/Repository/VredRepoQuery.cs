using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository
{
    public class VredRepoQuery : IRequest<bool>
    {
        public CreateCustomerEntities CreateCustomerEntities { get; set; }
        public VredRepoQuery(CreateCustomerEntities createCustomerEntities)
        {
            CreateCustomerEntities = createCustomerEntities;
        }
    }
}
