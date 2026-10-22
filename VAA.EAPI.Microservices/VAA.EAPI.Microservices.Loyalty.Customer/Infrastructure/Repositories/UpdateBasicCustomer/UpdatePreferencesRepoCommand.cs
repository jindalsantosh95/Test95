using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.UpdateBasicCustomer
{
    public class UpdatePreferencesRepoCommand : IRequest<bool>
    {
        public CreateCustomerEntities CreateCustomerEntities { get; set; }
        public UpdatePreferencesRepoCommand(CreateCustomerEntities createCustomerEntities)
        {
            CreateCustomerEntities = createCustomerEntities;
        }
    }
}
