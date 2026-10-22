using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository
{
    public interface IPartnerDocumentRepository
    {
        public Task AddPartnerDetails(CreateCustomerEntities createCustomerEntities);
        public Task UpdatePartnerDetails(CreateCustomerEntities createCustomerEntities, PartnerMembership partnerMembership);
    }
}
