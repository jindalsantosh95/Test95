using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository
{
    public interface IMemberProfileRepository
    {
        public Task AddMemberProfileDetails(CreateCustomerEntities customerEventResponse);
        public Task UpdateMemberProfileDetails(CreateCustomerEntities createCustomerEntities, MemberProfile customers);

    }
}
