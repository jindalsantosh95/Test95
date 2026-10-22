using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories
{
    public class FetchDocumentDetailsQuery : IRequest<bool>
    {
        public CreateCustomerEntities CreateCustomerEntities { get; set; }
        public FetchDocumentDetailsQuery(CreateCustomerEntities createCustomerEntities)
        {
            CreateCustomerEntities = createCustomerEntities;
        }
    }
}
