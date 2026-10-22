using MediatR;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class IdentityRepoCommand : IRequest<bool>
    {
        public CreateCustomerEntities CreateCustomerEntities { get; set; }
        public bool IsAccountEnabled { get; set; }
        public IdentityRepoCommand(CreateCustomerEntities createCustomerEntities, bool isAccountEnabled = true)
        {
            CreateCustomerEntities = createCustomerEntities;
            IsAccountEnabled = isAccountEnabled;
        }
    }
}

